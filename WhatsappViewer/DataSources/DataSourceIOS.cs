using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace WhatsappViewer.DataSources
{
    class DataSourceIOS : IDataSource
    {

        private string fileName;
        private SQLiteConnection sqlite_connection;

        private List<IOSChatItem> Chats { get; set; }
        private List<IOSMessageItem> Messages { get; set; }

        public static DateTime unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

        public DataSourceIOS(string DatabaseFilePath)
        {
            fileName = System.IO.Path.GetTempFileName();
            System.IO.File.WriteAllBytes(fileName, System.IO.File.ReadAllBytes(DatabaseFilePath));
            sqlite_connection = new SQLiteConnection("Data Source=" + fileName + ";Version=3;New=True;Compress=True;", true);
            sqlite_connection.Open();
        }

        ~DataSourceIOS()
        {
            try
            {
                System.IO.File.Delete(fileName);
                sqlite_connection.Dispose();
            }
            catch (Exception) { }
        }

        public IEnumerable<IMessageItem> getMessages(string ChatName)
        {
            using (var sqlite_command = sqlite_connection.CreateCommand())
            {
                sqlite_command.CommandText =
                "SELECT M.Z_PK, M.ZMESSAGESTATUS, M.ZMESSAGETYPE, M.ZMESSAGEDATE, M.ZPUSHNAME, " + 
                "    M.ZTEXT, M.ZTOJID, M.ZFROMJID,  C.ZCONTACTNAME, C.ZMEMBERJID " +
                "FROM ZWAMESSAGE AS M " +
                "    LEFT JOIN ZWAGROUPMEMBER AS C ON M.ZGROUPMEMBER = C.Z_PK " +
                "WHERE M.ZCHATSESSION = " + ChatName;

                SQLiteDataReader sqlite_datareader = sqlite_command.ExecuteReader();
                Messages = new List<IOSMessageItem>();
                while (sqlite_datareader.Read())
                {
                    var values = sqlite_datareader.GetValues();

                    var item = new IOSMessageItem();
                    item.Z_PK = int.Parse("0" + values["Z_PK"] + "");
                    item.ZMESSAGESTATUS = int.Parse("0" + values["ZMESSAGESTATUS"] + "");
                    item.ZMESSAGETYPE = int.Parse("0" + values["ZMESSAGETYPE"] + "");
                    item.ZMESSAGEDATE = TimeStampToDateTime(double.Parse("0" + values["ZMESSAGEDATE"], CultureInfo.InvariantCulture));
                    item.ZFROMJID = values["ZFROMJID"] + "";
                    item.ZPUSHNAME = values["ZPUSHNAME"] + "";
                    item.ZTEXT = values["ZTEXT"] + "";
                    item.ZTOJID = values["ZTOJID"] + "";
                    item.ZCONTACTNAME = values["ZCONTACTNAME"] + "";
                    item.ZMEMBERJID = values["ZMEMBERJID"] + "";

                    Messages.Add(item);
                }

                return Messages.OrderBy(o => o.ZMESSAGEDATE).ToList();

            }
        }

        public static DateTime? TimeStampToDateTime(double ts)
        {
            if (ts > 0)
            {
                return unixEpoch + TimeSpan.FromSeconds(978307200 + ts);
            }
            return null;
        }

        public IEnumerable<ChatItem> getChats()
        {

            var list = new List<ChatItem>();

            using (var sqlite_command = sqlite_connection.CreateCommand())
            {
                sqlite_command.CommandText = "SELECT * FROM ZWACHATSESSION;";
                SQLiteDataReader sqlite_datareader = sqlite_command.ExecuteReader();
                Chats = new List<IOSChatItem>();
                while (sqlite_datareader.Read())
                {
                    var item = new IOSChatItem();

                    var values = sqlite_datareader.GetValues();

                    item.Z_PK = int.Parse("0" + values["Z_PK"].ToString());
                    //item.Z_ENT = int.Parse("0" + values["Z_ENT"].ToString());
                    //item.Z_OPT = int.Parse("0" + values["Z_OPT"].ToString());
                    //item.ZCONTACTABID = int.Parse("0" + values["ZCONTACTABID"].ToString());
                    //item.ZFLAGS = int.Parse("0" + values["ZFLAGS"].ToString());
                    //item.ZMESSAGECOUNTER = int.Parse("0" + values["ZMESSAGECOUNTER"].ToString());
                    //item.ZUNREADCOUNT = int.Parse("0" + values["ZUNREADCOUNT"].ToString());
                    //item.ZGROUPINFO = int.Parse("0" + values["ZGROUPINFO"].ToString());
                    //item.ZLASTMESSAGE = int.Parse("0" + values["ZLASTMESSAGE"].ToString());
                    //item.ZPROPERTIES = int.Parse("0" + values["ZPROPERTIES"].ToString());
                    item.ZLASTMESSAGEDATE = TimeStampToDateTime(double.Parse("0" + values["ZLASTMESSAGEDATE"], CultureInfo.InvariantCulture));
                    item.ZCONTACTJID = values["ZCONTACTJID"].ToString();
                    //item.ZLASTMESSAGETEXT = values["ZLASTMESSAGETEXT"].ToString();
                    //item.ZPARTNERNAME = values["Z_PK"].ToString();
                    //item.ZSAVEDINPUT = values["ZSAVEDINPUT"].ToString();

                    Chats.Add(item);

                    list.Add(new ChatItem()
                    {
                        id = int.Parse("0" + values["Z_PK"].ToString()),
                        name = values["ZCONTACTJID"].ToString(),
                        descr = values["ZPARTNERNAME"].ToString(),
                    });


                }


            }

            return list;

        }

    }
}
