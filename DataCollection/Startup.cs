using Microsoft.Owin;
using Owin;
using Hangfire;
using DataCollection;
using System.Security.Cryptography;
using System;
using System.Text;
using System.Web.Configuration;
using System.Configuration;


[assembly: OwinStartupAttribute(typeof(DataCollection.Startup))]
namespace DataCollection
{
    public partial class Startup {
        public void Configuration(IAppBuilder app) {
            ConfigureAuth(app);
            
            //HangFire Configuration Begins
            GlobalConfiguration.Configuration.UseSqlServerStorage("ConStr");

            DataCollection.AutomaticBackup AutoBackup = new DataCollection.AutomaticBackup();//instantiating back up class

            //Scheduling background backups
            RecurringJob.AddOrUpdate(() => AutoBackup.backupQuestionPapersDump(), "0 5/12 * * *", TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));
            RecurringJob.AddOrUpdate(() => AutoBackup.backupPapers(), "0 5/12 * * *", TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));
            RecurringJob.AddOrUpdate(() => AutoBackup.backupCAPapers(), "0 5/12 * * *", TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));//cron for backing up twice a day
            RecurringJob.AddOrUpdate(() => AutoBackup.backupHardPapers(), "0 5/12 * * *", TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));//cron for backing up twice a day
            RecurringJob.AddOrUpdate(() => new RepetitionsManager().updateRepetionsKeeper(), Cron.Hourly , TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));//cron for updating RepetitionsKeeper every hour
            RecurringJob.AddOrUpdate(() => AutoBackup.cleanDBofTempTable(), "0 5/12 * * *", TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));//cron for backing up twice a day

            //Scheduling background backups Ends

            /*
             * Logs may later be changed to logging in a text file
             * Both logs can be maintained
             * text file logging can help back logging even when SQL server is down
             */

            app.UseHangfireDashboard();
            app.UseHangfireServer();
            ////HangFire Configuration Ends



            /*Setting Static manual keys for viewstate validation
             * Setting this to avoid the validation Message Authentication Code failure problem
             * MUST BE REVIEWED FOR SECURITY LATER
             * Machine key in the configuration file is manipulated in the following section
             * Using SHA1 for validation of the key and encryption
             * Generating a 128 bits key manually
             */
            /*
          System.Diagnostics.Debug.WriteLine("Start Key generation");

          RNGCryptoServiceProvider KeyGenerator = new RNGCryptoServiceProvider();
          int KeyLength = 64;

          byte[] tempKeyStoreBuffer = new byte[KeyLength / 2];//buffer to store key temporarily during building key

          KeyGenerator.GetBytes(tempKeyStoreBuffer);

          StringBuilder CustomKey = new StringBuilder(KeyLength);

          for (int c = 0; c < tempKeyStoreBuffer.Length; c++)
              CustomKey.Append(string.Format("{0:X2}",tempKeyStoreBuffer[c]));
          System.Diagnostics.Debug.WriteLine("This generated: -"+CustomKey+"-");

          System.Diagnostics.Debug.WriteLine("Finish Key generation");


          System.Diagnostics.Debug.WriteLine("-----Adding key to the configuration file-----");
          //Adding manually generated Machine Key to the configuration file
          System.Diagnostics.Debug.WriteLine("Opening config file");
          Configuration DataCollectionConfig = WebConfigurationManager.OpenWebConfiguration("~");//Empty string refers to the path of the configuration file in the root of the application
          System.Diagnostics.Debug.WriteLine("Accessing machine key section");
          MachineKeySection MKsec = (MachineKeySection)DataCollectionConfig.GetSection("system.web/machineKey");



          System.Diagnostics.Debug.WriteLine("present validation key: -"+MKsec.ValidationKey+"-"+"\nSetting machine key");
          MKsec.ValidationKey = CustomKey.ToString();//Setting customkey as the machine validation key

          System.Diagnostics.Debug.WriteLine("present decryption key: -"+MKsec.DecryptionKey+"-"+"\nSetting decryption key");
          MKsec.DecryptionKey = CustomKey.ToString();//Setting customkey as the machine decryption key

          System.Diagnostics.Debug.WriteLine("present  validation mode: -" + MKsec.Validation+ "-" + "\nSetting validation mode");
          MKsec.Validation = MachineKeyValidation.SHA1;//setting validation as SHA1

          if (!MKsec.SectionInformation.IsLocked)
          {
              //  DataCollectionConfig.Save(ConfigurationSaveMode.Modified);//saving changes to configuration file
              DataCollectionConfig.Save();
            //  ConfigurationManager.RefreshSection("system.web/machineKey");
              System.Diagnostics.Debug.WriteLine("Settings updated");
              System.Diagnostics.Debug.WriteLine("New Machine Valdation: -"+MKsec.Validation+"-");
              System.Diagnostics.Debug.WriteLine("New Valdation Key: -"+MKsec.ValidationKey+"-");
              System.Diagnostics.Debug.WriteLine("New Decryption Key: -"+MKsec.DecryptionKey+"-");
          }
          else
          {//section locked
          System.Diagnostics.Debug.WriteLine("Could not update because section is locked");
          }


          //ENDS//Adding manually generated Machine Key to the configuration file
          System.Diagnostics.Debug.WriteLine("-----Adding key to the configuration file ENDS-----");
          */

        }
    }
}
