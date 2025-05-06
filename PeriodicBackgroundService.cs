using ZoumaFinance.Controllers;

namespace ZoumaAttendanceApplication.Controllers
{
	public class PeriodicBackgroundService : BackgroundService
	{
		private readonly IServiceScopeFactory _scopeFactory;
		private Timer _dailyTimer;
		private Timer _dailyTimernew;
		private Timer _tuesday9AMTimer;
        private Timer _fourtyMinuteTimer;
		private Timer _fifteenMinuteTimer;
		private Timer _dailyFirstWeekTimer;
		private Timer _hourlyTimer;
		private Timer _noonTimer; // Add a timer for noon tasks
		private Timer _tuesdayTimer;
		private Timer _tuesday10AMTimer;
		private readonly ILogger<PeriodicBackgroundService> _loger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PeriodicBackgroundService(IServiceScopeFactory serviceScopeFactory,ILogger<PeriodicBackgroundService> loger, IHttpContextAccessor httpContextAccessor)
		{
			_scopeFactory = serviceScopeFactory;
			_loger = loger;
			_httpContextAccessor = httpContextAccessor;


        }

		protected override Task ExecuteAsync(CancellationToken stoppingToken)
		{
			_fourtyMinuteTimer = new Timer(DoWork, "Every40Minutes", TimeSpan.Zero, TimeSpan.FromMinutes(40));
			_fifteenMinuteTimer = new Timer(DoWork, "Every15Minutes", TimeSpan.Zero, TimeSpan.FromMinutes(15));
			_hourlyTimer = new Timer(DoWork, "Hourly", TimeSpan.Zero, TimeSpan.FromHours(1));
			ScheduleDailyWork();
			ScheduleDailyFirstWeekWork();
			ScheduleNoonWork(); // Schedule the noon task
			ScheduleTuesdayWorkAt9AM();
			ScheduleTuesdayWorkAt10AM();
            ScheduleDailyWorkAt3AM();
			ScheduleTuesdayWork();
            return Task.CompletedTask;
		}

		private void ScheduleDailyWork()
		{
			var now = DateTime.Now;
            //var nextRun = new DateTime(now.Year, now.Month, now.Day, 9, 0, 0); // 9 AM today
            var nextRun = new DateTime(now.Year, now.Month, now.Day, 4, 0, 0); // 2:00 AM today
          //  var nextRun = new DateTime(now.Year, now.Month, now.Day, 18, 15, 0); // 5:15 PM today


            if (now > nextRun)
			{
				nextRun = nextRun.AddDays(1); // Schedule for 2 AM tomorrow if it's past 2 AM today
			}
			var initialDelay = nextRun - now;
			_dailyTimernew = new Timer(DoWork, "Daily", initialDelay, TimeSpan.FromDays(1));
		}
        private void ScheduleDailyWorkAt3AM()
        {
            var now = DateTime.Now;
            var nextRun = new DateTime(now.Year, now.Month, now.Day, 3, 0, 0); // 3:00 AM today
          //  var nextRun = new DateTime(now.Year, now.Month, now.Day, 11, 5, 0); // 11:05 AM today


            if (now > nextRun)
            {
                nextRun = nextRun.AddDays(1); // Schedule for 3:00 AM tomorrow if it's past 3:00 AM today
            }

            var initialDelay = nextRun - now; // Time span until the next run
            _dailyTimer = new Timer(DoWork, "Daily at 3 AM", initialDelay, TimeSpan.FromDays(1)); // Schedule for 24-hour intervals
        }
        private void ScheduleDailyFirstWeekWork()
		{
			var now = DateTime.Now;
			if (now.Day >= 1 && now.Day <= 7)
			{
				var nextRun = new DateTime(now.Year, now.Month, now.Day, 9, 5, 0); // 9:5:0 AM today
				if (now > nextRun)
				{
					nextRun = nextRun.AddDays(1); // Schedule for 9 AM tomorrow if it's past 9 AM today
				}
				var initialDelay = nextRun - now;
				_dailyFirstWeekTimer = new Timer(DoWork, "DailyFirstWeek", initialDelay, TimeSpan.FromDays(1));
			}
		}

		private void ScheduleNoonWork()
		{
			var now = DateTime.Now;
			var nextRun = new DateTime(now.Year, now.Month, now.Day, 12, 0, 0); // 12 PM (noon) today
			if (now > nextRun)
			{
				nextRun = nextRun.AddDays(1); // Schedule for 12 PM tomorrow if it's past 12 PM today
			}
			var initialDelay = nextRun - now;
			_noonTimer = new Timer(DoWork, "Noon", initialDelay, TimeSpan.FromDays(1));
		}
        private void ScheduleTuesdayWork()
        {
            var now = DateTime.Now;
            var nextRun = new DateTime(now.Year, now.Month, now.Day, 8, 0, 0); // 8 AM today
            //var nextRun = new DateTime(now.Year, now.Month, now.Day, 17, 35, 0); // 8 AM today

            // Find the next Tuesday
            while (nextRun.DayOfWeek != DayOfWeek.Tuesday || now > nextRun)
            {
                nextRun = nextRun.AddDays(1);
            }

            var initialDelay = nextRun - now;
            _tuesdayTimer = new Timer(DoWork, "Tuesday", initialDelay, TimeSpan.FromDays(7));
        }
        private void ScheduleTuesdayWorkAt9AM()
        {
            var now = DateTime.Now;
            var nextRun = new DateTime(now.Year, now.Month, now.Day, 9, 0, 0); // 9 AM today
																			   //var nextRun = new DateTime(now.Year, now.Month, now.Day, 14, 05, 0); // 1:35 PM today


			// Find the next Tuesday at 9 AM
			while (nextRun.DayOfWeek != DayOfWeek.Tuesday || now > nextRun)
			{
				nextRun = nextRun.AddDays(1);
			}

			var initialDelay = nextRun - now;
			_tuesday9AMTimer = new Timer(DoWork, "Tuesday9AM", initialDelay, TimeSpan.FromDays(7));
		}
        private void ScheduleTuesdayWorkAt10AM()
        {
            var now = DateTime.Now;
            var nextRun = new DateTime(now.Year, now.Month, now.Day, 10, 0, 0); // 10 AM today

            // Find the next Tuesday at 10 AM
            while (nextRun.DayOfWeek != DayOfWeek.Tuesday || now > nextRun)
            {
                nextRun = nextRun.AddDays(1);
            }

            var initialDelay = nextRun - now;
            _tuesday10AMTimer = new Timer(DoWork, "Tuesday10AM", initialDelay, TimeSpan.FromDays(7));
        }

        private void DoWork(object state)
		{
			var taskType = (string)state;
			CallApiAsync(taskType).GetAwaiter().GetResult();
		}

		private async Task CallApiAsync(string taskType)
		{
			using (var scope = _scopeFactory.CreateScope())
			{
				var scopedService = scope.ServiceProvider.GetRequiredService<EmployeeController>();
				var scopedServicenew= scope.ServiceProvider.GetRequiredService<NCHistoryController>();

				// Call GenerateAccessToken first

				if (taskType == "Daily")
				{
					await scopedService.EmployeeData();
					//	await scopedService.TimeSheetAPIForCurrentMonth();

				}
				else if (taskType == "Daily at 3 AM")
				{
					await scopedService.AttendanceAPI();
					//await scopedService.GenerateAccessToken();
					//await scopedService.ProductivityAPI();
				}
				else if (taskType == "Every40Minutes")
				{
					//await scopedService.GenerateAccessToken();
					//await scopedService.ProductivityAPI();
				}
				else if (taskType == "Every15Minutes")
				{
					//await scopedService.AttendanceAPI();
				}
				else if (taskType == "DailyFirstWeek")
				{
					if (DateTime.Now.Day >= 1 && DateTime.Now.Day <= 7)
					{
						//	await scopedService.TimeSheetAPIForPreviousMonth();
					}
				}
				else if (taskType == "Tuesday")
                {
               
                    
                    await scopedService.TimeSheetAPIForCurrentMonth();

                }
				else if (taskType == "Tuesday9AM")
				{
					//await scopedServicenew.GenerateNCHistoryPdf();

				}
                else if (taskType == "Tuesday10AM")
                {
                    //await scopedServicenew.GenerateAndSendNCHistoryPdf();


                }
                else if (taskType == "Hourly")
				{
					// You can add specific tasks for the hourly timer here
					// await scopedService.AttendanceAPI();
				}
				else if (taskType == "Noon")
				{
					//await scopedService.LeavesAPI();
				}
			}
		}

		public override Task StopAsync(CancellationToken cancellationToken)
		{
			_dailyTimer?.Change(Timeout.Infinite, 0);
            _dailyTimernew?.Change(Timeout.Infinite, 0);
            _fourtyMinuteTimer?.Change(Timeout.Infinite, 0);
			_fifteenMinuteTimer?.Change(Timeout.Infinite, 0);
			_dailyFirstWeekTimer?.Change(Timeout.Infinite, 0);
            _tuesdayTimer?.Change(Timeout.Infinite, 0);
            _tuesday9AMTimer?.Change(Timeout.Infinite, 0);
            _hourlyTimer?.Change(Timeout.Infinite, 0);
			_noonTimer?.Change(Timeout.Infinite, 0); // Stop the noon timer
            _tuesday10AMTimer?.Change(Timeout.Infinite, 0);

            return base.StopAsync(cancellationToken);
		}

		public override void Dispose()
		{
			_dailyTimer?.Dispose();
            _dailyTimernew?.Dispose();
            _fourtyMinuteTimer?.Dispose();
			_fifteenMinuteTimer?.Dispose();
			_dailyFirstWeekTimer?.Dispose();
            _tuesdayTimer?.Dispose();
			_tuesday9AMTimer?.Dispose();
			_tuesday10AMTimer?.Dispose();
            _hourlyTimer?.Dispose();
			_noonTimer?.Dispose(); // Dispose of the noon timer
			base.Dispose();
		}
	}

}
