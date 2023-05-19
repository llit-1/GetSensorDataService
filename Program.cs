﻿using GetSensorDataService;
using GetSensorDataService.DB;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;


string LogPath = "C:\\GetSensorDataService\\logs.txt";
string JsonPath = "C:\\GetSensorDataService\\appconfig.json";
try
{
    File.AppendAllText(LogPath, $"\n{DateTime.Now} Старт службы GetSensorDataServise"); //логируем запуск приложения
}
catch (Exception ex)
{
    File.AppendAllText("logs.txt", $"\n{DateTime.Now} {ex.Message}"); 
}


while (true) //приложение выполняет 1 бесконечный цикл
{
    string connectionString = "";
    int Interval = 0;
    try
    {
        string json = File.ReadAllText(JsonPath); // достаём настройки из appconfig.json
        Settings settings = JsonSerializer.Deserialize<Settings>(json);
        connectionString = settings.ConnectionString;
        Interval = settings.Interval;
    }

    catch (Exception ex)
    {
        File.AppendAllText(LogPath, $"\n{DateTime.Now} {ex.Message}");
        Thread.Sleep(60000);
        continue;
    }
    try
    {
        DbContextOptionsBuilder<GetSensorDataService.DB.MSSQLContext> dbContextOptionsBuilder = new(); // создаём настройки подключения к бд
        dbContextOptionsBuilder.UseSqlServer(connectionString);

        using (MSSQLContext db = new(dbContextOptionsBuilder.Options)) // подключаемся к бд
        {
            foreach (var item in db.SensorRooms) // для каждого датчика из комнаты выполняем запрос
            {

                SensorData sensorData = new SensorData();
                sensorData.RoomId = item.Id;
                sensorData.RoomName = item.Name;
                sensorData.Temperature = GetData.GetTemperature(item.Ip);
                sensorData.Humidity = GetData.GetHumidity(item.Ip);
                sensorData.Date = DateTime.Now;
                db.Add(sensorData);
                db.SaveChanges(); // сохраняем данные           
            }
            File.AppendAllText(LogPath, $"\n{DateTime.Now} Данные успешно сохранены"); //логируем успешное завершение
        }
    }
    catch (Exception ex)
    {
        File.AppendAllText(LogPath, $"\n{ex.Message}");
    }
    Thread.Sleep(Interval); // делаем паузу на время указанное в appconfig.json
}



