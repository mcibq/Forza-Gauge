using ForzaGauge.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForzaGauge.Commands
{
    public class DashboardRpmPlusCommand : CommandBase
    {
        private readonly DashboardViewModel _dashboardViewModel;
        public DashboardRpmPlusCommand(DashboardViewModel dashboardViewModel)
        {
            _dashboardViewModel = dashboardViewModel;
        }

        public override void Execute(object parameter)
        {
            _dashboardViewModel.RpmPlus();
        }
    }
}
