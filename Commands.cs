using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace BUS_DAA_SIGMA
{
    interface ICommand
    {
        void Execute();
    }

    interface ICommandAsync : ICommand
    {
        Task ExecuteAsync();
    }

#region Commands
    class ConnectCommand : ICommand
    {
        private void _execute()
        {
            UI.Print("Give a remote end point address: ");
            var address = Console.ReadLine();
            UI.Print("Give a remote end point port: ");
            var port = Console.ReadLine();

            try
            {
                User.Connect(IPAddress.Parse(address), int.Parse(port));
            }
            catch (Exception ex)
            {
                UI.Print(ex.Message);
            }
        }

        public void Execute() => _execute();
    }

    class SendCommand : ICommand
    {
        private void _execute()
        {
            UI.Print("Give a remote end point address: ");
            var address = Console.ReadLine();
            UI.Print("Give a remote end point port: ");
            var port = Console.ReadLine();
            UI.Print("Give a message: ");
            var message = Console.ReadLine();

            try
            {
                User.Send(IPAddress.Parse(address), int.Parse(port), Encoding.ASCII.GetBytes(message));
            }
            catch (Exception ex)
            {
                UI.Print(ex.Message);
            }
        }

        public void Execute() => _execute();
    }

    class PortCommand : ICommand
    {
        private void _execute() => UI.Print(User.Receiver.LocalEndPoint.Port.ToString());

        public void Execute() => _execute();
    }
#endregion
#region Factories
    abstract class Factory
    {
        public abstract ICommand Create();
    }

    class ConnectFactory : Factory
    {
        public override ICommand Create() => new ConnectCommand();
    }

    class SendFactory : Factory
    {
        public override ICommand Create() => new SendCommand();
    }

    class PortFactory : Factory
    {
        public override ICommand Create() => new PortCommand();
    }
#endregion

    public static class Commands
    {
        private enum Actions
        {
            Connect,
            Send,
            Port
        }

        private static readonly Dictionary<Actions, Factory> _factories;

        static Commands()
        {
            _factories = new Dictionary<Actions, Factory>();

            foreach(Actions action in Enum.GetValues(typeof(Actions)))
            {
                var factory = (Factory)Activator.CreateInstance(Type.GetType("BUS_DAA_SIGMA." + Enum.GetName(typeof(Actions), action) + "Factory"));
                _factories.Add(action, factory);
            }
        }

        private static ICommand ExecuteCreation(Actions action) => _factories[action].Create();

        public static void Decode(string command)
        {
            var factory = ExecuteCreation((Actions)Enum.Parse(typeof(Actions), command));
            factory.Execute();
        }
    }
}
