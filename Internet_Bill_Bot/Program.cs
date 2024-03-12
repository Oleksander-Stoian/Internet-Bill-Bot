using System;
using System.Collections.Generic;
<<<<<<< HEAD
using System.Reflection.Metadata;
=======
>>>>>>> 7071668f26a35e6639d5aa8ac20220153f88e464
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

<<<<<<< HEAD

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
        AwaitingComplaint
    }

    static Dictionary<long, UserState> userStates = new Dictionary<long, UserState>();
=======
using Microsoft.EntityFrameworkCore;
using static System.Net.Mime.MediaTypeNames;

public class ApplicationDbContext : DbContext
{
    public DbSet<Application> Applications { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseMySql("server=localhost;database=telegram_bot_bd;user=root;password=Qaz186186",
            new MySqlServerVersion(new Version(8, 0, 36))); // Вказуйте актуальну версію MySQL сервера
    }
}
public class Application
{
    public int Id { get; set; }
    public long UserId { get; set; }
    public int ApartmentNumber { get; set; }
    public string Complaint { get; set; }
    public DateTime Date { get; set; } // Час подання заявки буде встановлено автоматично
    public string FirstName { get; set; } // Ім'я
    public string LastName { get; set; } // Прізвище
    public string Patronymic { get; set; } // По батькові
}

// Структура для збереження персональних даних користувача
class PersonalInfo
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Patronymic { get; set; }
}



class Program 
{
   
    static ITelegramBotClient botClient;
    // Словник для збереження стану користувачів
    static Dictionary<long, bool> awaitingComplaint = new Dictionary<long, bool>();
    // Словник для збереження стану, що очікує номер квартири
    static Dictionary<long, int> apartmentNumbers = new Dictionary<long, int>();

    // Словник для тимчасового збереження номера квартири
    static Dictionary<long, bool> awaitingApartmentNumber = new Dictionary<long, bool>();
    //Словник з ФІП
    static Dictionary<long, bool> awaitingPersonalInfo = new Dictionary<long, bool>();
    // Словник для тимчасового збереження персональних даних користувачів
    static Dictionary<long, PersonalInfo> personalInfos = new Dictionary<long, PersonalInfo>();

>>>>>>> 7071668f26a35e6639d5aa8ac20220153f88e464

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
<<<<<<< HEAD
            if (!userStates.ContainsKey(chatId))
            {
                userStates[chatId] = UserState.None; // Initialize with None state if not exist
            }
            if (messageText.Equals("/start", StringComparison.InvariantCultureIgnoreCase))
            {
                ReplyKeyboardMarkup replyKeyboardMarkup = new(new[]
                {
                new KeyboardButton[] { "Залишити заявку", "Переглянути мої заявки" },
            })
                {
                    ResizeKeyboard = true,
                    OneTimeKeyboard = true,
=======
            if (messageText.Equals("/start", StringComparison.InvariantCultureIgnoreCase))
            {
                var replyKeyboardMarkup = new ReplyKeyboardMarkup(new[]
                {
                new KeyboardButton[] { "Розпочати" }
            })
                {
                    ResizeKeyboard = true,
                    OneTimeKeyboard = true, // Зробити клавіатуру одноразовою (зникне після вибору)
>>>>>>> 7071668f26a35e6639d5aa8ac20220153f88e464
                };

                await botClient.SendTextMessageAsync(
                    chatId: chatId,
<<<<<<< HEAD
                    text: "Виберіть опцію:",
=======
                    text: "Натисніть кнопку 'Розпочати', щоб почати.",
>>>>>>> 7071668f26a35e6639d5aa8ac20220153f88e464
                    replyMarkup: replyKeyboardMarkup,
                    cancellationToken: cancellationToken
                );
            }
<<<<<<< HEAD
            // Реакція на вибір "Залишити заявку"
            else if (messageText.Equals("Залишити заявку", StringComparison.InvariantCultureIgnoreCase))
            {
                userStates[chatId] = UserState.AwaitingDocumentNumber;

                await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: "Введіть номер договору (4 цифри):",
                    cancellationToken: cancellationToken
                );
            }
            else
            {
                switch (userStates[chatId])
                {
                    case UserState.AwaitingDocumentNumber:
                        if (int.TryParse(messageText, out int documentNumber))
                        {
                            // Зберігаємо номер документу
                            documentNumbers[chatId] = documentNumber;

                            // Змінюємо стан на очікування адреси
                            userStates[chatId] = UserState.AwaitingApartmentNumber;

                            await botClient.SendTextMessageAsync(chatId, "Введіть адресу вашого будинку:", cancellationToken: cancellationToken);
                        }
                        else
                        {
                            await botClient.SendTextMessageAsync(chatId, "Будь ласка, введіть коректний номер договору (тільки чотири числа):", cancellationToken: cancellationToken);
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
                            await botClient.SendTextMessageAsync(chatId, "Будь ласка, введіть коректний номер квартири (тільки числа):", cancellationToken: cancellationToken);
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
                            userStates[chatId] = UserState.AwaitingComplaint;

                            await botClient.SendTextMessageAsync(chatId, "Тепер введіть вашу скаргу:", cancellationToken: cancellationToken);
                        }
                        else
                        {
                            // Якщо користувач не надав достатньо інформації, просимо зробити це знову
                            await botClient.SendTextMessageAsync(chatId, "Будь ласка, введіть ваше ім'я, прізвище та по батькові через пробіл.", cancellationToken: cancellationToken);
                        }
                        break;


                    case UserState.AwaitingComplaint:

                        if (apartmentNumbers.TryGetValue(chatId, out int aptNumber) &&
                        personalInfos.TryGetValue(chatId, out PersonalInfo pInfo) &&
                        documentNumbers.TryGetValue(chatId, out int docNumber)) // Ensure the document number is also retrieved
                        {
                            // Save the complaint and the document number in the database
                            using (var context = new ApplicationDbContext())
                            {
                                var application = new Application
                                {
                                    UserId = chatId,
                                    DocumentNumber = docNumber, // Now includes the document number
                                    ApartmentNumber = aptNumber,
                                    Complaint = messageText,
                                    Date = DateTime.UtcNow,
                                    FirstName = pInfo.FirstName,
                                    LastName = pInfo.LastName,
                                    Patronymic = pInfo.Patronymic,
                                };
                                context.Applications.Add(application);
                                await context.SaveChangesAsync();
                            }

                            // Скидання стану та очищення збережених даних
                            userStates[chatId] = UserState.None;
                            apartmentNumbers.Remove(chatId);
                            personalInfos.Remove(chatId);

                            await botClient.SendTextMessageAsync(chatId, "Дякую, Ваша проблема буде вирішена.", cancellationToken: cancellationToken);
                        }
                        else
                        {
                            // Якщо не вдалося отримати всі необхідні дані
                            await botClient.SendTextMessageAsync(chatId, "Сталася помилка при обробці вашої скарги. Спробуйте знову.", cancellationToken: cancellationToken);
                        }
                        break;
                    default:
                        // Якщо стан невідомий, можливо, варто нагадати користувачу, що йому робити
                        break;
                }
            }
 
            }
        
=======
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
>>>>>>> 7071668f26a35e6639d5aa8ac20220153f88e464
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
