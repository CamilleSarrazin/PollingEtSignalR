using labo.signalr.api.Data;
using Microsoft.AspNetCore.SignalR;

namespace labo.signalr.api.Hubs
{
    public static class UserHandler
    {
        public static HashSet<string> ConnectedIds = new HashSet<string>();
    }

    public class TaskHub:Hub
    {
        ApplicationDbContext _context;

        public TaskHub(ApplicationDbContext context)
        {
            _context = context;
        }

        public override async Task OnConnectedAsync()
        {
            //Déclencher la fonction TaskList sur le client qui a fait l’appel
            await base.OnConnectedAsync();
            UserHandler.ConnectedIds.Add(Context.ConnectionId);
            await Clients.All.SendAsync("UserCount", UserHandler.ConnectedIds.Count);
            await Clients.Caller.SendAsync("TaskList", _context.UselessTasks.ToList());
        }

        public async Task AddTask(string task)
        {
            //Ajouter une nouvelle tâche dans la BD, puis déclencher la fonction TaskList sur tous les clients
            _context.UselessTasks.Add(new Models.UselessTask() { Text = task });
            _context.SaveChanges();
            await Clients.All.SendAsync("TaskList", _context.UselessTasks.ToList());
        }

        public async Task CompleteTask(int taskid)
        {
            //Marquer une tâche comme complétée dans BD, puis déclencher la fonction TaskList sur tous les clients
            var task = _context.UselessTasks.Single(t => t.Id == taskid);
            task.Completed = true;
            _context.SaveChanges();
            await Clients.All.SendAsync("TaskList", _context.UselessTasks.ToList());
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            //Décrémenter le nombre d'utilisateurs actifs
            UserHandler.ConnectedIds.Remove(Context.ConnectionId);
            //Déclencher la fonction UserCount sur les clients
            await Clients.All.SendAsync("UserCount", UserHandler.ConnectedIds.Count);
            await base.OnDisconnectedAsync(exception);
        }
    }
}
