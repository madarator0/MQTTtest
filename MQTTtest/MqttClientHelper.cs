using MQTTnet;
using MQTTnet.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PKTI.Persistence.Services
{
    public class MqttClientHelper
    {
        private IMqttClient _mqttClient;
        private readonly MqttClientOptions mqttClientOptions;
        private readonly MqttClientDisconnectOptions mqttClientDisconnectOptions;
        private byte koddat;
        private string topic;
        private TaskCompletionSource<bool> koddatMatchedTcs; // Используем для ожидания сообщения с нужным koddat

        // Конструктор для настройки клиента
        public MqttClientHelper(string URL, int port, byte koddat, string topic)
        {
            this.koddat = koddat;

            mqttClientOptions = new MqttClientOptionsBuilder()
                .WithTcpServer(URL, port) // Адрес и порт брокера
                .WithCleanSession() // Очищаем сессию при каждом подключении
                .Build();

            // Параметры отключения
            mqttClientDisconnectOptions = new MqttClientDisconnectOptionsBuilder()
                .WithReasonString("Manual Disconnect") // Сообщение при отключении
                .Build();

            this.topic = topic;
        }

        // Метод для подключения к брокеру
        public async Task ConnectAsync()
        {
            // Создаем MQTT фабрику для создания клиента
            var mqttFactory = new MqttFactory();
            _mqttClient = mqttFactory.CreateMqttClient();

            // Подключаемся к брокеру
            await _mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);
            Console.WriteLine("MQTT клиент подключен к брокеру.");

            // Обработка сообщений из топиков
            _mqttClient.ApplicationMessageReceivedAsync += async e =>
            {
                Console.WriteLine("Получено сообщение: ");
                Console.WriteLine($"Тема: {e.ApplicationMessage.Topic}");
                Console.WriteLine($"Сообщение: {BitConverter.ToString(e.ApplicationMessage.Payload)}");

                // Проверяем, содержит ли сообщение нужный koddat
                if (e.ApplicationMessage.Payload != null && e.ApplicationMessage.Payload.Length > 0 && e.ApplicationMessage.Payload[0] == koddat)
                {
                    Console.WriteLine($"Сообщение с нужным koddat {koddat} получено.");
                    koddatMatchedTcs?.TrySetResult(true); // Сообщаем, что сообщение с нужным koddat получено
                }
                else
                {
                    Console.WriteLine($"Сообщение с другим koddat: {e.ApplicationMessage.Payload[0]}");
                }

                await Task.CompletedTask;
            };

            // Обработка отключения
            _mqttClient.DisconnectedAsync += e =>
            {
                Console.WriteLine("Клиент отключен от брокера.");
                return Task.CompletedTask;
            };
        }

        // Метод для выполнения опроса с отправкой второго сообщения только после получения нужного koddat
        public async Task Opros()
        {
            string data = ProtocolData.CreateRandom(koddat, 1).ToString(); /*"K:04;F:01;Q:31;B:3562;A:01;R:42BB06;T:25.02.14,10:57:20";*/
            // Создаем первое сообщение, используя закодированное время
            await PublishMessageAsync(data, topic);
        }

        // Метод для подписки на несколько топиков
        public async Task SubscribeToTopicsAsync()
        {
            var subscribeOptionsBuilder = new MqttClientSubscribeOptionsBuilder();
            subscribeOptionsBuilder.WithTopicFilter(topic + "/1");

            var subscribeOptions = subscribeOptionsBuilder.Build();
            await _mqttClient.SubscribeAsync(subscribeOptions);
            Console.WriteLine("Подписка на топики завершена.");
        }

        // Метод для отправки сообщения в указанный топик
        public async Task PublishMessageAsync(string payload, string topic)
        {
            var message = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(payload)
                .Build();

            await _mqttClient.PublishAsync(message, CancellationToken.None);
            Console.WriteLine($"Сообщение отправлено в топик '{topic}': {payload}");
        }

        // Метод для отключения от брокера
        public async Task DisconnectAsync()
        {
            if (_mqttClient != null && _mqttClient.IsConnected)
            {
                await _mqttClient.DisconnectAsync(mqttClientDisconnectOptions, CancellationToken.None);
                Console.WriteLine("MQTT клиент отключен от брокера.");
            }
        }
    }
}
