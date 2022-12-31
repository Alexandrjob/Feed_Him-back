using Microsoft.AspNetCore.SignalR;
using TaskTrackerCat.Repositories.Models;

namespace TaskTrackerCat.Infrastructure;

public class DietHub : Hub
{
    // Список пользователей, подключенных к хабу
    private static List<string> Users = new List<string>();

    // Метод, который вызывается клиентом при подключении
    public void OnConnectedAsync(string userName)
    {
        var user = Users.FirstOrDefault(u => u == userName);
        if (user != null)
        {
            return;
        }

        // Добавляем пользователя в список
        Users.Add(userName);
    }

    // Метод, который вызывается клиентом при отключении
    public void OnDisconnectedAsync(string userName)
    {
        // Удаляем пользователя из списка
        Users.Remove(userName);
    }

    [HubMethodName("privateMethod")]
    // Метод, который вызывается клиентом для отправки сообщения
    public async Task UpdateDietAsync(DietDto model)
    {
        if (Clients == null)
        {
            return;
        }

        // Отправляем сообщение всем клиентам
        await Clients.All.SendAsync("UpdateDiet", model);
    }
}