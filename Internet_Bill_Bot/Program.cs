using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Text;
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

    enum UserState
    {
        None,
        AwaitingDocumentNumber,
        AwaitingApartmentNumber,
        AwaitingPersonalInfo,
        AwaitingComplaintSelection,
        AwaitingCustomComplaint // Доданий стан для "Інше"
    }

    static Dictionary<long, UserState> userStates = new Dictionary<long, UserState>();

    static async Task DisplayUserApplications(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken)
    {
        using (var context = new ApplicationDbContext())
        {
            var userApplications = await context.Applications.Where(a => a.UserId == chatId).ToListAsync();

            if (userApplications.Any())
            {
                var message = new StringBuilder("Ваші заявки:\n");
                foreach (var app in userApplications)
                {
                    message.AppendLine($"Документ №{app.DocumentNumber}, Квартира: {app.ApartmentNumber}, Скарга: {app.Complaint}, Дата: {app.Date.ToString("dd/MM/yyyy")}");
                }
                await botClient.SendTextMessageAsync(chatId, message.ToString(), cancellationToken: cancellationToken);
            }
            else
            {
                await botClient.SendTextMessageAsync(chatId, "У вас немає зареєстрованих заявок.", cancellationToken: cancellationToken);
            }
        }
    }


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
                 cancellationToken: cts.Token);


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

            // Ініціалізуємо стан користувача, якщо він ще не встановлений
            if (!userStates.ContainsKey(chatId))
            {
                userStates[chatId] = UserState.None;
            }

            // Основна логіка для першого входу користувача
            if (messageText.Equals("/start", StringComparison.InvariantCultureIgnoreCase))
            {
                var replyMarkup = new ReplyKeyboardMarkup(new[]
                {
                new KeyboardButton[] {"Залишити заявку"},
                new KeyboardButton[] {"Переглянути мої заявки"}
            })
                {
                    ResizeKeyboard = true
                };
                await botClient.SendTextMessageAsync(chatId, "Виберіть опцію:", replyMarkup: replyMarkup, cancellationToken: cancellationToken);
                return; // Після відправки /start повідомлення виходимо з метода
            }

            // Делегуємо подальшу обробку в HandleStateDependentMessage
            await HandleStateDependentMessage(botClient, chatId, messageText, cancellationToken);
        }
    }


    static async Task HandleStateDependentMessage(ITelegramBotClient botClient, long chatId, string messageText, CancellationToken cancellationToken)
    {
        // Перевірка вибору "Переглянути мої заявки" на початку, щоб уникнути конфлікту станів
        if (messageText.Equals("Переглянути мої заявки", StringComparison.InvariantCultureIgnoreCase))
        {
            await botClient.SendTextMessageAsync(chatId, "Ваші заявки:", cancellationToken: cancellationToken);
            await DisplayUserApplications(botClient, chatId, cancellationToken);
            return;
        }
        switch (userStates[chatId])
        {
            case UserState.None:
                if (messageText.Equals("Залишити заявку", StringComparison.InvariantCultureIgnoreCase))
                {
                    userStates[chatId] = UserState.AwaitingDocumentNumber;
                    await botClient.SendTextMessageAsync(chatId, "Введіть номер договору (4 цифри):", cancellationToken: cancellationToken);
                }
                break;

            case UserState.AwaitingDocumentNumber:
                if (int.TryParse(messageText, out int documentNumber) && messageText.Length == 4)
                {
                    documentNumbers[chatId] = documentNumber;
                    userStates[chatId] = UserState.AwaitingApartmentNumber;
                    await botClient.SendTextMessageAsync(chatId, "Введіть номер вашого гуртожитку (1-24):", cancellationToken: cancellationToken);
                }
                else
                {
                    await botClient.SendTextMessageAsync(chatId, "Будь ласка, введіть коректний номер договору (тільки чотири цифри):", cancellationToken: cancellationToken);
                }
                break;

            case UserState.AwaitingApartmentNumber:
                if (int.TryParse(messageText, out int apartmentNumber))
                {
                    // Зберігаємо номер квартири
                    apartmentNumbers[chatId] = apartmentNumber;

                    // Змінюємо стан на очікування персональних даних
                    userStates[chatId] = UserState.AwaitingPersonalInfo;

                    await botClient.SendTextMessageAsync(chatId, "Тепер введіть ваше ім'я, прізвище та по батькові:", cancellationToken: cancellationToken);
                }
                else
                {
                    await botClient.SendTextMessageAsync(chatId, "Будь ласка, введіть коректний номер гуртожитку 1-24 (тільки числа):", cancellationToken: cancellationToken);
                }
                break;

            case UserState.AwaitingPersonalInfo:
                string[] personalInfoParts = messageText.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (personalInfoParts.Length >= 3)
                {
                    // Створення нового екземпляру персональних даних
                    PersonalInfo persInfo = new PersonalInfo(personalInfoParts[0], personalInfoParts[1], personalInfoParts[2]);

                    // Зберігаємо персональні дані
                    personalInfos[chatId] = persInfo;

                    // Змінюємо стан на очікування скарги
                    userStates[chatId] = UserState.AwaitingComplaintSelection;
                    var replyKeyboardMarkup = new ReplyKeyboardMarkup(new[]
                    {
                        new [] { new KeyboardButton("Не працює роутер") },
                        new [] { new KeyboardButton("Не працює розетка комп'ютерна") },
                        new [] { new KeyboardButton("Слабкий сигнал інтернету") },
                        new [] { new KeyboardButton("Не працює вайфай") },
                        new [] { new KeyboardButton("Інше") }
                    })
                    {
                        ResizeKeyboard = true,
                        OneTimeKeyboard = true
                    };
                    await botClient.SendTextMessageAsync(chatId, "Будь ласка, виберіть проблему зі списку:", replyMarkup: replyKeyboardMarkup, cancellationToken: cancellationToken);
                    break;
                }
                else
                {
                    // Якщо користувач не надав достатньо інформації, просимо зробити це знову
                    await botClient.SendTextMessageAsync(chatId, "Будь ласка, введіть ваше ім'я, прізвище та по батькові через пробіл.", cancellationToken: cancellationToken);
                }
                break;
                case UserState.AwaitingComplaintSelection:
                if (messageText.Equals("Інше", StringComparison.InvariantCultureIgnoreCase))
                {
                    userStates[chatId] = UserState.AwaitingCustomComplaint;
                    await botClient.SendTextMessageAsync(chatId, "Будь ласка, опишіть вашу проблему:", cancellationToken: cancellationToken);
                }
                else
                {
                    // Для будь-якого іншого вибору з клавіатури зберігаємо вибір як Complaint
                    var selectedComplaint = messageText; // Вибір користувача з клавіатури
                    SaveApplication(chatId, selectedComplaint);
                }
                break;

            case UserState.AwaitingCustomComplaint:
                // В цьому стані будь-яке повідомлення від користувача вважається заявкою
                var customComplaint = messageText; // Текст заявки користувача
                SaveApplication(chatId, customComplaint);
                break;

            // Ваші інші case умови...

            default:
                // Якщо стан користувача None або не підтримується
               
                if (messageText.Equals("Переглянути мої заявки", StringComparison.InvariantCultureIgnoreCase))
                {
                    await DisplayUserApplications(botClient, chatId, cancellationToken);
                }
                else if (messageText.Equals("Залишити заявку", StringComparison.InvariantCultureIgnoreCase))
                {
                    userStates[chatId] = UserState.AwaitingComplaintSelection;
                    var replyKeyboardMarkup = new ReplyKeyboardMarkup(new[]
                     {
                        new KeyboardButton[] {"Не працює роутер", "Не працює розетка комп'ютерна", "Слабкий сигнал інтернету", "Не працює вайфай", "Інше"}
                    })
                    {
                        ResizeKeyboard = true,
                        OneTimeKeyboard = true,
                    };
                    await botClient.SendTextMessageAsync(chatId, "Будь ласка, виберіть проблему зі списку:", replyMarkup: replyKeyboardMarkup, cancellationToken: cancellationToken);
                }
                break;
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
    private static void ResetUserData(long chatId)
    {
        userStates[chatId] = UserState.None;
        apartmentNumbers.Remove(chatId);
        personalInfos.Remove(chatId);
        documentNumbers.Remove(chatId);
    }
    private static async void SaveApplication(long chatId, string complaint)
    {
        using (var context = new ApplicationDbContext())
        {
            var application = new Application
            {
                UserId = chatId,
                DocumentNumber = documentNumbers.ContainsKey(chatId) ? documentNumbers[chatId] : 0,
                ApartmentNumber = apartmentNumbers.ContainsKey(chatId) ? apartmentNumbers[chatId] : 0,
                Complaint = complaint,
                Date = DateTime.UtcNow,
                FirstName = personalInfos.ContainsKey(chatId) ? personalInfos[chatId].FirstName : "",
                LastName = personalInfos.ContainsKey(chatId) ? personalInfos[chatId].LastName : "",
                Patronymic = personalInfos.ContainsKey(chatId) ? personalInfos[chatId].Patronymic : "",
            };
            context.Applications.Add(application);
            await context.SaveChangesAsync();
        }

        // Скидання стану та очищення даних
        ResetUserData(chatId);

        // Відправляємо повідомлення з подякою та клавіатурою для наступних дій
        await botClient.SendTextMessageAsync(chatId, "Дякую, ваша проблема буде вирішена.", replyMarkup: new ReplyKeyboardMarkup(new[]
        {
        new KeyboardButton[] {"Залишити заявку"},
        new KeyboardButton[] {"Переглянути мої заявки"}
    })
        {
            ResizeKeyboard = true,
            OneTimeKeyboard = true
        });
    }

}
