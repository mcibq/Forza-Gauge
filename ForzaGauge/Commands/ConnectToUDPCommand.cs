using ForzaGauge.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForzaGauge.Commands
{
    public class ConnectToUDPCommand : CommandBase
    {
        private bool _canExecuteUDP = true;
        public bool CanExecuteUDP
        {
            get { return _canExecuteUDP; }
            set { _canExecuteUDP = value; OnCanExecutedChanged(); }
        }
        private readonly DashboardViewModel _dashboardViewModel;
        public ConnectToUDPCommand(DashboardViewModel dashboardViewModel)
        {
            _dashboardViewModel = dashboardViewModel;
        }

        public override void Execute(object parameter)
        {
            if (CanExecuteUDP)
            {
                _dashboardViewModel.Connect();
                CanExecuteUDP = false;
            }
        }
        public override bool CanExecute(object parameter)
        {
            
            return CanExecuteUDP;
        }
    }
}
