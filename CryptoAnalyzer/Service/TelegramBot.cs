using Microsoft.Extensions.Hosting;
using StackExchange.Exceptional;
using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;

namespace CryptoAnalyzer.Service
{
	public class TelegramBot : IHostedService
	{
		private static TelegramBotClient _telegramBotClient;

		public Task StartAsync(CancellationToken cancellationToken)
		{
			_telegramBotClient = new TelegramBotClient(Context.TelegramBotConfiguration.APIKey);
			_telegramBotClient.OnMessage += Bot_OnMessage;
			_telegramBotClient.StartReceiving(cancellationToken: cancellationToken);

			return Task.CompletedTask;
		}

		public Task StopAsync(CancellationToken cancellationToken)
		{
			_telegramBotClient.StopReceiving();

			return Task.CompletedTask;
		}

		private static async void Bot_OnMessage(object sender, MessageEventArgs e)
		{
			if (e.Message.Text != null)
			{
				Console.WriteLine($"Received a text message in chat {e.Message.Chat.Id}.");
				await _telegramBotClient.SendTextMessageAsync(
				  chatId: e.Message.Chat,
				  text: "ConversationID:\n" + e.Message.Chat.Id
				);
			}
		}

		public async Task SendMessageAsync(long chatID, string message, bool disableNotification = false)
		{
			try
			{
				await _telegramBotClient.SendTextMessageAsync(chatID, message, disableNotification: disableNotification);
			}
			catch(Exception e)
			{
				e.LogNoContext();
			}
		}
	}
}
