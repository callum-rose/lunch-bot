﻿using Microsoft.Graph;
using Serilog;

namespace LunchBot;

public class ChatOrchestrator
{
	private readonly TitleAuthor _titleAuthor;
	private readonly MessageAuthor _messageAuthor;
	private readonly AvailableDateMessageAuthor _availableDateMessageAuthor;
	private readonly VenueChooser _venueChooser;
	private readonly ChatHandler _chatHandler;
	private readonly ILogger _logger;

	public ChatOrchestrator(TitleAuthor titleAuthor,
		MessageAuthor messageAuthor,
		AvailableDateMessageAuthor availableDateMessageAuthor,
		VenueChooser venueChooser,
		ChatHandler chatHandler,
		ILogger logger)
	{
		_titleAuthor = titleAuthor;
		_messageAuthor = messageAuthor;
		_availableDateMessageAuthor = availableDateMessageAuthor;
		_venueChooser = venueChooser;
		_chatHandler = chatHandler;
		_logger = logger;
	}

	public async Task<LunchData> DeliverAll(bool dryRun,
		int seed,
		User conductor,
		string partyName,
		PartyData partyData)
	{
		if (!Blocker.RequestUserCodeVerification("Enter code to launch"))
		{
			_logger.Warning("Incorrect code entered. Cancelling launch");
			return new LunchData(partyData.Id, dryRun, false, Array.Empty<GroupChat>());
		}

		(bool success, IReadOnlyList<GroupChat> chats) =
			await DeliverAllInternal(dryRun, seed, conductor, partyName, partyData.Party);

		return new LunchData(partyData.Id, dryRun, success, chats);
	}

	private async Task<(bool success, IReadOnlyList<GroupChat> chats)> DeliverAllInternal(bool dryRun,
		int seed,
		User conductor,
		string partyName,
		Party party)
	{
		_logger.Information("Starting to deliver all chats");

		string chatTitle = _titleAuthor.GetTitle(partyName);

		GroupChat[] chats = new GroupChat[party.Groups.Count];

		await _venueChooser.Initialise(seed);

		for (int i = 0; i < party.Groups.Count; i++)
		{
			Group group = party.Groups[i];

			(bool successfulCreation, string chatId) =
				await _chatHandler.TryCreateChat(dryRun, conductor, chatTitle, group);

			if (!successfulCreation)
			{
				return (false, chats);
			}

			string venue = _venueChooser.GetVenue(i);

			chats[i] = new GroupChat(group.Users, group.Number, chatId, venue);

			string message = _messageAuthor.CreateInitialChatMessage(group, venue);

			bool successfulMessage = await _chatHandler.TrySendMessage(dryRun, chatId, message);

			if (!successfulMessage)
			{
				return (false, chats);
			}
			
			foreach (string dateMessage in _availableDateMessageAuthor.GetMessages(DateTime.Today))
			{
				bool successfulDateMessage = await _chatHandler.TrySendMessage(dryRun, chatId, dateMessage);

				if (!successfulDateMessage)
				{
					return (false, chats);
				}
			}
		}

		return (true, chats);
	}
}