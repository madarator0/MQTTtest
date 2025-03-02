using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PKTI.Persistence.Services;

class Program
{
    static async Task Main(string[] args)
    {
        string mqttBrokerUrl = "127.0.0.1";
        int mqttPort = 1883;  // Example port 

        // Список для хранения всех клиентов
        List<MqttClientHelper> mqttClientHelpers = new List<MqttClientHelper>();

        mqttClientHelpers.Add(new MqttClientHelper(mqttBrokerUrl, mqttPort, 1, "F01/D/O", 1));
        //mqttClientHelpers.Add(new MqttClientHelper(mqttBrokerUrl, mqttPort, 5, "F01/D/O", 1));
        mqttClientHelpers.Add(new MqttClientHelper(mqttBrokerUrl, mqttPort, 1, "F02/G/O", 2));

        //// Создаем 15 клиентов с топиком "test/topic2"
        //for (int i = 0; i < 15; i++)
        //{
        //    var client = new MqttClientHelper(mqttBrokerUrl, mqttPort, (byte)i, "test/topic2");
        //    mqttClientHelpers.Add(client);
        //}

        // Подключаем всех клиентов и подписываем их на топики
        foreach (var clientHelper in mqttClientHelpers)
        {
            await clientHelper.ConnectAsync();
            await clientHelper.SubscribeToTopicsAsync();

            await Task.Delay(1000);
            // Пример вызова метода opros для публикации сообщений
            await clientHelper.Opros();
        }

        Console.WriteLine("Клиенты подключены и подписаны на топики.");
        Console.ReadLine();

        // Отключаем клиентов при завершении программы
        foreach (var clientHelper in mqttClientHelpers)
        {
            await clientHelper.DisconnectAsync();
        }

        Console.WriteLine("Клиенты отключены.");
    }
}
