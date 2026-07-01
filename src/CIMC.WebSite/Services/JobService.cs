using AutoMapper;
using CIMC.Data;
using CIMC.EntityFramework;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Nito.AsyncEx;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CimcSite.Web.Services
{
    /// <summary>
    /// 
    /// </summary>
    public class JobService : BackgroundService
    {
        private IMapper _mapper;
        private readonly IConfiguration _config;
        //private IRepository<Admin> _adminRepository;
        private readonly IServiceProvider _serviceProvider;
        private readonly AsyncLock _mutex = new AsyncLock();

        public JobService(
            IMapper mapper, IConfiguration config,
            IServiceProvider serviceProvider
            //IRepository<Admin> adminRepository
            )
        {
            _mapper = mapper;
            _config = config;
            // _adminRepository = adminRepository;
            //using (var scope = serviceProvider.CreateScope())
            //{
            //    _adminRepository = scope.ServiceProvider.GetRequiredService<IRepository<Admin>>();
            //}
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                //异步锁            
                using (await _mutex.LockAsync())
                {
                    var times = Convert.ToInt32(300); //回调的时间间隔(秒)
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var _adminRepository = scope.ServiceProvider.GetRequiredService<IRepository<Admin>>();
                        _adminRepository.GetOne(p => p.UserName == "admin");
                    }

                    await Task.Delay(TimeSpan.FromSeconds(times), stoppingToken);
                    Console.WriteLine("回调服务 > " + DateTime.Now);

                }
            }
        }


    }
}
