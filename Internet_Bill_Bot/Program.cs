using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;


namespace Internet_Bill_Bot;

class Program 
{

    static ITelegramBotClient botClient;

    // Словник для збереження стану користувачів
    static Dictionary<long, bool> awaitingComplaint = new Dictionary<long, bool>();

    // Словник для збереження стану, що очікує номер квартири
    static Dictionary<long, int> apartmentNumbers = new Dictionary<long, int>();
    // Словник для тимчасового збереження номера квартири
    static Dictionary<long, bool> awaitingApartmentNumber = new Dictionary<long, bool>();

    // Словник для тимчасового збереження номера документа 
    static Dictionary<long, bool> awaitingDocumentNumber = new Dictionary<long, bool>();
    // Словник для збереження стану, що очікує номер квартири
    static Dictionary<long, int> documentNumbers = new Dictionary<long, int>();

    //Словник з ФІП
    static Dictionary<long, bool> awaitingPersonalInfo = new Dictionary<long, bool>();

    // Словник для тимчасового збереження персональних даних користувачів
    static Dictionary<long, PersonalInfo> personalInfos = new Dictionary<long, PersonalInfo>();


    static async Task Main()
    {
        botClient = new TelegramBotClient("6940735278:AAE0tDrA4X2sVH2lFLnDgbEW1uPtewBUqW0");
        var cts = new CancellationTokenSource();

        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = Array.Empty<UpdateType>() // Приймати всі типи оновлень
        };

        botClient.StartReceiving(
            HandleUpdateAsync,
            HandleErrorAsync,
            receiverOptions,
            cts.Token);

        Console.WriteLine($"Bot is up and running.");
        Console.ReadLine();

        cts.Cancel();
    }

    static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (update.Type == UpdateType.Message && update.Message.Type == MessageType.Text)
        {
            var chatId = update.Message.Chat.Id;
            var messageText = update.Message.Text;
            if (messageText.Equals("/start", StringComparison.InvariantCultureIgnoreCase))
            {
                ReplyKeyboardMarkup replyKeyboardMarkup = new(new[]
{
                    new KeyboardButton[] { "Help me" },
                    new KeyboardButton[] { "Call me ☎️" },
                })
                {
                    ResizeKeyboard = true,
                    OneTimeKeyboard = true, // Зробити клавіатуру одноразовою (зникне після вибору)
                };

                await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: "Ведіть номер договору (4 цифри):",
                    cancellationToken: cancellationToken
                );
            }
            // Перевірка, чи користувач ввів номер договору
            if (awaitingApartmentNumber.TryGetValue(chatId, out bool isAwaitingDocumentNumber) && isAwaitingDocumentNumber)
            {
                if (int.TryParse(messageText, out int documentNumber))
                {
                    documentNumbers[chatId] = documentNumber; // Зберігаємо номер квартири
                    awaitingDocumentNumber[chatId] = false; // Скидаємо стан
                    awaitingPersonalInfo[chatId] = true; // Перехід до стану очікування персональних даних
                    await botClient.SendTextMessageAsync(chatId,  "Введіть адрусу вашого будинку:", cancellationToken: cancellationToken);
                }
                else
                {
                    await botClient.SendTextMessageAsync(chatId, "Будь ласка, введіть коректний номер договру (тільки чотири числа):", cancellationToken: cancellationToken);
                }
                return;
            }

            // Перевірка, чи користувач ввів номер квартири
            if (awaitingApartmentNumber.TryGetValue(chatId, out bool isAwaitingApartmentNumber) && isAwaitingApartmentNumber)
            {
                if (int.TryParse(messageText, out int apartmentNumber))
                {
                    apartmentNumbers[chatId] = apartmentNumber; // Зберігаємо номер квартири
                    awaitingApartmentNumber[chatId] = false; // Скидаємо стан
                    awaitingPersonalInfo[chatId] = true; // Перехід до стану очікування персональних даних
                    await botClient.SendTextMessageAsync(chatId, "Тепер введіть ваше ім'я, прізвище та по батькові:", cancellationToken: cancellationToken);
                }
                else
                {
                    await botClient.SendTextMessageAsync(chatId, "Будь ласка, введіть коректний номер квартири (тільки числа):", cancellationToken: cancellationToken);
                }
                return;
            }

            // Перевірка, чи користувач ввів персональні дані
            if (awaitingPersonalInfo.TryGetValue(chatId, out bool isAwaitingPersonalInfo) && isAwaitingPersonalInfo)
            {
                awaitingPersonalInfo[chatId] = false; // Скидаємо стан
                string[] personalInfoParts = messageText.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (personalInfoParts.Length >= 3)
                {
                    var personalInfo = new PersonalInfo
                    {
                        FirstName = personalInfoParts[0],
                        LastName = personalInfoParts[1],
                        Patronymic = personalInfoParts[2]
                    };
                    personalInfos[chatId] = personalInfo; // Зберігаємо персональні дані

                    awaitingComplaint[chatId] = true; // Перехід до стану очікування скарги
                    await botClient.SendTextMessageAsync(chatId, "Тепер введіть вашу скаргу:", cancellationToken: cancellationToken);
                }
                else
                {
                    awaitingPersonalInfo[chatId] = true; // Знову встановлюємо стан очікування персональних даних
                    await botClient.SendTextMessageAsync(chatId, "Будь ласка, введіть ім'я, прізвище та по батькові через пробіл.", cancellationToken: cancellationToken);
                }
                return;
            }

            if (awaitingComplaint.TryGetValue(chatId, out bool isAwaitingComplaint) && isAwaitingComplaint)
            {
                int apartmentNumber = apartmentNumbers[chatId];
                var personalInfo = personalInfos[chatId]; // Витягуємо персональні дані

                // Зберігаємо скаргу в базі даних
                using (var context = new ApplicationDbContext())
                {
                    var application = new Application
                    {
                        UserId = chatId,
                        ApartmentNumber = apartmentNumber,
                        Complaint = messageText,
                        Date = DateTime.UtcNow,
                        FirstName = personalInfo.FirstName,
                        LastName = personalInfo.LastName,
                        Patronymic = personalInfo.Patronymic
                    };
                    context.Applications.Add(application);
                    await context.SaveChangesAsync();
                }
                awaitingComplaint[chatId] = false; // Скидаємо стан
                personalInfos.Remove(chatId); // Видаляємо збережені персональні дані
                await botClient.SendTextMessageAsync(chatId, "Дякую, Ваша проблема буде вирішена.", cancellationToken: cancellationToken);
            }

            if (messageText.Equals("/start", StringComparison.InvariantCultureIgnoreCase))
            {
                var inlineKeyboard = new InlineKeyboardMarkup(new[]
                {
                InlineKeyboardButton.WithCallbackData("Залишити заявку", "apply")
            });

                await botClient.SendTextMessageAsync(chatId, "Виберіть дію:", replyMarkup: inlineKeyboard, cancellationToken: cancellationToken);
            }
        }
        else if (update.Type == UpdateType.CallbackQuery)
        {
            var callbackQuery = update.CallbackQuery;
            var chatId = callbackQuery.Message.Chat.Id;

            if (callbackQuery.Data == "apply")
            {
                awaitingApartmentNumber[chatId] = true; // Встановлюємо стан "чекаємо номер квартири"
                await botClient.SendTextMessageAsync(chatId, "Введіть номер вашої квартири:", cancellationToken: cancellationToken);

                // Опціонально: видаліть інлайнову клавіатуру
                await botClient.EditMessageReplyMarkupAsync(chatId, callbackQuery.Message.MessageId, replyMarkup: null, cancellationToken: cancellationToken);
            }
        }
    }


    static Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        var errorMessage = exception switch
        {
            ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };

        Console.WriteLine(errorMessage);
        return Task.CompletedTask;
    }
}
