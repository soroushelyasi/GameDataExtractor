﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GameDataExtractor
{
    class Program
    {
        static void Main(string[] args)
        {
            /*------------------------------------------------------------------------------------------------------*/
            /*########################################## initial variable ##########################################*/
            /*------------------------------------------------------------------------------------------------------*/
            string path = @"I:\مقاله\مقاله 2\data of game";
            bool isCsvInput = true;
            int inputNumber = 2;
            MethoudToRun methoudToRun = MethoudToRun.RecordBasedFutureExtractor;
            /*------------------------------------------------------------------------------------------------------*/
            /*########################################## initial variable ##########################################*/
            /*------------------------------------------------------------------------------------------------------*/
            List<string> countableFeachers, keyValueFeachers;
            List<List<string>> listWordFeatchers;
            switch (inputNumber)
            {
                case 1:
                    Input1(out countableFeachers, out keyValueFeachers, out listWordFeatchers);
                    break;
                case 2:
                    Input2(out countableFeachers, out keyValueFeachers, out listWordFeatchers);
                    break;
                default:
                    throw new IndexOutOfRangeException();
            }

            List<string> resaultList = new List<string>();

            AddHeadersIfCsv(isCsvInput, countableFeachers, keyValueFeachers, resaultList, listWordFeatchers);

            foreach (string file in Directory.EnumerateFiles(path, "*.xml", SearchOption.AllDirectories))
            {
                GC.Collect();

                string temporalResault = "";

                temporalResault = CreatePading(temporalResault, isCsvInput);

                if (!file.ToLower().Contains("u_"))
                    continue;

                string fileNameLower = file.ToLower();
                temporalResault = BasicInfo(file, temporalResault, fileNameLower, isCsvInput);
                string fileText = File.ReadAllText(file).ToLower();
                string[] refinedText;

                switch (methoudToRun)
                {
                    case MethoudToRun.MoveLRCalculator:
                        refinedText = Refiner(fileText, methoudToRun);
                        temporalResault = MoveLRCalculator(isCsvInput, temporalResault, refinedText);
                        break;
                    case MethoudToRun.Main:
                        refinedText = Refiner(fileText, methoudToRun);
                        temporalResault = Main(isCsvInput, countableFeachers, keyValueFeachers, listWordFeatchers, temporalResault, refinedText);
                        break;
                    case MethoudToRun.FileSize:
                        temporalResault = FileSize(file, temporalResault);
                        break;
                    case MethoudToRun.TimeCalculator:
                        TimeCalculator();
                        break;
                    case MethoudToRun.MaxScoreFinder:
                        refinedText = Refiner(fileText, methoudToRun);
                        temporalResault = MaxScoreFinder(isCsvInput, temporalResault, refinedText);
                        break;
                    case MethoudToRun.RecordBasedFutureExtractor:
                        temporalResault = RecordBasedFutureExtractor(isCsvInput, temporalResault, fileText);
                        break;
                    default:
                        throw new IndexOutOfRangeException();
                }

                if (isCsvInput && temporalResault[temporalResault.Length - 1] == ',')
                    temporalResault = temporalResault.Substring(0, temporalResault.Length - 1);

                if (!isCsvInput)
                    temporalResault += "\n";
                Console.WriteLine(temporalResault);
                resaultList.Add(temporalResault);
            }

            resaultList = SortResList(resaultList);

            System.IO.File.WriteAllLines(path + "\\" + (isCsvInput ? "resaultOfCounting.csv" : "resaultOfCounting.txt"), resaultList);
            Console.WriteLine("Done !!!");
            Console.ReadKey();

        }

        private static string RecordBasedFutureExtractor(bool isCsvInput, string temporalResault, string fileText)
        {

            var listWordFromTos = new List<List<string>>() {
                new List<string>() { "preTuterial DoorLock1", "preTuterial Matching" }.ConvertAll(d => d.ToLower()),
                new List<string>() { "preTuterial Matching", "preTuterial GalexyShooter" }.ConvertAll(d => d.ToLower()),
                new List<string>() { "preTuterial GalexyShooter", "preTuterial DoorLock2" }.ConvertAll(d => d.ToLower()),
                new List<string>() { "preTuterial DoorLock2", "preTuterial Hack" }.ConvertAll(d => d.ToLower()),
                new List<string>() { "preTuterial Hack", "Finish Page" }.ConvertAll(d => d.ToLower())};

            foreach (var listWordFromTo in listWordFromTos)
            {
                var text = fileText.Substring(fileText.IndexOf(listWordFromTo[0]), fileText.IndexOf(listWordFromTo[1]) - fileText.IndexOf(listWordFromTo[0]));
                var refinedText = Refiner(text, MethoudToRun.RecordBasedFutureExtractor);
                var count = refinedText.Count();

                DateTime timeStart = new DateTime(), timeFinish = new DateTime();
                double totalMinuts;
                Regex rx = new Regex(@"\d{1,2}\/\d{1,2}\/\d{4}\s\d{1,2}\:\d{1,2}\:\d{1,22}");
                Match m = rx.Match(refinedText[1]);
                if (m.Success)
                {
                    try
                    {
                        timeStart = DateTime.ParseExact(m.Value.Trim(), "M/d/yyyy hh:mm:ss", CultureInfo.InvariantCulture);
                    }
                    catch (Exception ex)
                    {
                        if (ex.Message.Equals("String was not recognized as a valid DateTime."))
                            try
                            {
                                timeStart = DateTime.ParseExact(m.Value.Trim(), "M/d/yyyy h:mm:ss", CultureInfo.InvariantCulture);
                            }
                            catch (Exception ex2)
                            {
                                try
                                {
                                    timeStart = DateTime.ParseExact(m.Value.Trim(), "d/M/yyyy h:mm:ss", CultureInfo.InvariantCulture);
                                }
                                catch (Exception ex3)
                                {
                                    var temph = int.Parse(m.Value.Trim().Substring(m.Value.Trim().IndexOf(':') - 2, 2));
                                    var newtime = m.Value.Replace(temph + ":", (temph - 12) + ":").Trim();
                                    timeStart = DateTime.ParseExact(newtime, "d/M/yyyy h:mm:ss", CultureInfo.InvariantCulture);
                                }
                            }
                        else if (ex.Message.Equals("The DateTime represented by the string is not supported in calendar System.Globalization.GregorianCalendar."))
                            timeStart = DateTime.ParseExact(m.Value.Trim(), "d/M/yyyy hh:mm:ss", CultureInfo.InvariantCulture);
                        else
                            throw ex;
                    }
                }
                m = rx.Match(refinedText[refinedText.Length - 1]);
                if (m.Success)
                {
                    try
                    {
                        timeFinish = DateTime.ParseExact(m.Value.Trim(), "M/d/yyyy hh:mm:ss", CultureInfo.InvariantCulture);
                    }
                    catch (Exception ex)
                    {
                        if (ex.Message.Equals("String was not recognized as a valid DateTime."))
                            try
                            {
                                timeFinish = DateTime.ParseExact(m.Value.Trim(), "M/d/yyyy h:mm:ss", CultureInfo.InvariantCulture);
                            }
                            catch (Exception ex2)
                            {
                                try
                                {
                                    timeFinish = DateTime.ParseExact(m.Value.Trim(), "d/M/yyyy h:mm:ss", CultureInfo.InvariantCulture);
                                }
                                catch (Exception ex3)
                                {
                                    var temph = int.Parse(m.Value.Trim().Substring(m.Value.Trim().IndexOf(':') - 2, 2));
                                    var newtime = m.Value.Replace(temph + ":", (temph - 12) + ":").Trim();
                                    timeFinish = DateTime.ParseExact(newtime, "d/M/yyyy h:mm:ss", CultureInfo.InvariantCulture);
                                }
                            }
                        else if (ex.Message.Equals("The DateTime represented by the string is not supported in calendar System.Globalization.GregorianCalendar."))
                            timeFinish = DateTime.ParseExact(m.Value.Trim(), "d/M/yyyy hh:mm:ss", CultureInfo.InvariantCulture);
                        else
                            throw ex;
                    }
                }
                totalMinuts = ((timeFinish - timeStart).TotalMinutes);
                if (totalMinuts < 0)
                    totalMinuts = ((new DateTime(timeFinish.Year, timeFinish.Month, timeFinish.Day, 12, 0, 0) - timeFinish).TotalMinutes) + ((new DateTime(timeFinish.Year, timeFinish.Month, timeFinish.Day, 12, 0, 0) - timeFinish).TotalMinutes);

                temporalResault += isCsvInput ? count + "," + totalMinuts + "," : "number of log " + String.Join("|", listWordFromTo) + " : " + count + "\n" + "number of Minurtes " + String.Join("|", listWordFromTo) + " : " + totalMinuts + "\n";
            }

            return temporalResault;
        }

        private static string[] Refiner(string fileText, MethoudToRun methoudToRun)
        {
            switch (methoudToRun)
            {
                case MethoudToRun.FileSize:
                case MethoudToRun.Main:
                case MethoudToRun.MaxScoreFinder:
                case MethoudToRun.MoveLRCalculator:
                case MethoudToRun.TimeCalculator:
                    return fileText.Replace("\"", "").Replace("{", "").Replace("}", "").Replace(", y", "-").Split(new char[] { ',' });
                case MethoudToRun.RecordBasedFutureExtractor:
                    return fileText.Replace("{", "").Split(new char[] { '}' });
                default:
                    throw new IndexOutOfRangeException();
            }
        }

        private static List<string> SortResList(List<string> resaultList)
        {
            var userList = new List<string>() { "u_nader", "u_12", "u_parvaneh1000p", "u_Adel 542", "u_SoroushRefVaraste", "u_MinaRefArya", "u_Neda60", "u_Mohamad3444", "u_samira1234", "u_maryamnouri", "u_kiaresh7", "u_mahvash", "u_leilaabbasi", "u_Mitra cheraghi", "u_nasrin", "u_seamorgh", "u_SinaRefArya", "u_Alireza_elyasi", "u_marya", "u_AryaVaraste", "u_59", "u_60", "u_61", "u_64", "u_63", "u_62", "u_13", "u_4", "u_5", "u_7", "u_11", "u_3", "u_50", "u_17", "u_40", "u_30", "u_20", "u_asalbahri", "u_REZA AZIZIAN", "u_Hadiseh Mohebbi", "u_Manieh sarani", "u_zeinab asgari", "u_Negin Miri", "u_samira zaim", "u_asal", "u_setayesh", "u_fereshte.jani", "u_narges", "u_mena afshar", "u_zahra heydari", "u_sepideh_hydrpoor", "u_hasti.shirazian", "u_mohadse aliyari", "u_mohadese zamani", "u_nadia mohamadi", "u_zahra najafi", "u_zeinab yousefi", "u_elmira shokriyan fard", "u_maedeh doabi", "u_Mohadese gholami", "u_Mahsa Hamidi", "u_fateme", "u_elahe.gharbaghi", "u_fatima", "u_Hanieh bavandpur", "u_Zahra lotfi", "u_Mahdiye Khalilzade", "u_Mobina ghasemloo", "u_elaheh", "u_amirali", "u_Barbod Gholizadeh", "u_amirali yarloo", "u_Arman Farsa", "u_Arya Nemati", "u_omid reza ghorbani", "u_Arsam Mirzaei", "u_hiva hoorbod", "u_Taha Tahmaseb poor", "u_amir pasha shokouhi", "u_miremad", "u_Arshia soroush", "u_Kasra Moafi Moafi", "u_kasrairanshahi", "u_Matin badiee", "u_samkhaftani", "u_amir hosin", "u_JDMBOI", "u_Amirali Partovi", "u_Amir Arman Bayat", "u_alijaxy", "u_faraz khalili", "u_alitamjidzad", "u_Kasra Shahrisvand", "u_sadrashariati", "u_Sepanta Behzadi", "u_Sina Bozorgmehr", "u_Korosh zolgadr", "u_Arsham Rostami", "u_Iliya heidary", "u_AmirParsa Zamani Fakhar", "u_pouria razavifard", "u_toral derakhshi", "u_sepehr  bozorgmehr", "u_Ali rangbar", "u_Ashkan sohrabi", "u_Mahbod Rahmany", "u_SSgumS", "u_patrick", "u_hossein rahimi", "u_A-Mehdi Gomar", "u_mhmdltf", "u_mh_ak", "u_Mari", "u_Momo", "u_hadi foroughi", "u_ghghgh", "u_Marzieh Alidadi", "u_Hosein Shendabadi", "u_m reza a", "u_pooya fekri", "u_Sarah Attar", "u_amir amin", "u_amirhossein sheikholeslam", "u_zahra r", "u_mohamad003", "u_mostafa masumi", "u_aaaa", "u_Mrick", "u_Medivh", "u_arsd", "u_Mohammad Naseri", "u_Hosein", "u_cyberdon", "u_mh_eb5", "u_ahmadbat" };
            var FinalList = new List<string>();
            FinalList.Add(resaultList[0]);

            foreach (var user in userList)
            {
                FinalList.Add(resaultList.First(x => x.ToLower().Contains((user + ".x").ToLower())));
            }
            return FinalList;
        }

        private static string FileSize(string file, string temporalResault)
        {
            long length = new System.IO.FileInfo(file).Length;
            temporalResault += length + ",";
            return temporalResault;
        }

        private static string Main(bool isCsvInput, List<string> countableFeachers, List<string> keyValueFeachers, List<List<string>> listWordFeatchers, string temporalResault, string[] refinedText)
        {
            foreach (var countableFeacher in countableFeachers)
            {
                var count = (from word in refinedText
                             where word.Contains(countableFeacher)
                             select word).Count();
                temporalResault += isCsvInput ? count + "," : "number of " + countableFeacher + " : " + count + "\n";
            }

            foreach (var keyValueFeacher in keyValueFeachers)
            {
                var tempRsKV = refinedText.First(x => x.Contains(keyValueFeacher));
                temporalResault += isCsvInput ? tempRsKV + "," : "kV " + tempRsKV + "\n";
            }
            foreach (var listWordFeatcher in listWordFeatchers)
            {
                //int count = 0;
                //foreach (var w in refinedText)
                //{
                //    if (w.Contains(listWordFeatcher[0]) && w.Contains(listWordFeatcher[1]))
                //        count++;
                //}
                var count = (from word in refinedText
                             where listWordFeatcher.All(x => word.Contains(x))
                             select word).Count();


                temporalResault += isCsvInput ? count + "," : "number of " + String.Join("|", listWordFeatcher) + " : " + count + "\n";
            }

            return temporalResault;
        }

        private static string MoveLRCalculator(bool isCsvInput, string temporalResault, string[] refinedText)
        {

            var moveList = refinedText.Where(x => x.Contains("location : ^loc^")).Select(x =>
            {
                var newX = x.Replace("whathappend:", "").Trim().Replace("location : ^loc^(x=", "");
                int index = newX.Contains("y=") ? newX.IndexOf("y=") : newX.IndexOf("-=");

                return newX.Substring(0, index).Replace("/", ".");
            }).Where(x => double.TryParse(x, out _)).Select(x => (int)(double.Parse(x) * 10)).ToList();

            var res = moveList.Select((elem, index) =>
            {
                if (index == 0)
                {
                    return "S";
                }

                if (elem == moveList[index - 1])
                {
                    return "S";
                }
                else if (elem < moveList[index - 1])
                {
                    return "L";
                }
                else
                {
                    return "R";
                }
            });

            var countLeftMove = res.Count(x => x == "L");
            var countRightMove = res.Count(x => x == "R");

            temporalResault += isCsvInput ? (countLeftMove + "," + countRightMove + "," + (countLeftMove + countRightMove) + ",") :
                "number of Left Move : " + countLeftMove + "\n" + "number of Right Move : " + countRightMove + "\n" + "number of Move : " + (countLeftMove + countRightMove) + "\n";

            return temporalResault;
        }



        private static void TimeCalculator()
        {
            while (true)
            {
                Console.WriteLine("start");
                var start = Console.ReadLine().Trim().Split(new char[] { ':' });
                Console.WriteLine("end");
                var end = Console.ReadLine().Split(new char[] { ':' });
                Console.WriteLine("end");
                DateTime date1 = new DateTime(2023, 1, 1, Int32.Parse(start[0]), Int32.Parse(start[1]), Int32.Parse(start[2]));
                DateTime date2 = new DateTime(2023, 1, 1, Int32.Parse(end[0]), Int32.Parse(end[1]), Int32.Parse(end[2]));
                Console.WriteLine("res: " + ((date2 - date1).TotalMinutes));
                Console.WriteLine("________________________________________");
            }
        }

        private static string MaxScoreFinder(bool isCsvInput, string temporalResault, string[] refinedText)
        {

            var maxScore = refinedText.Where(x => x.Contains("score: #")).Select(x =>
            {
                var text = x.Substring(x.IndexOf("score: #")).Replace("score: #", "").Trim();
                return Int32.Parse(text.Substring(0, text.IndexOf(" ")));

            }).Max();

            temporalResault += isCsvInput ? maxScore + "," : "maxScore " + maxScore + "\n";
            return temporalResault;
        }

        private static void AddHeadersIfCsv(bool isCsvInput, List<string> countableFeachers, List<string> keyValueFeachers, List<string> resaultList, List<List<string>> listWordFeatchers)
        {
            if (isCsvInput)
            {
                string headers = "file address,username,";
                foreach (var countableFeacher in countableFeachers)
                {
                    headers += countableFeacher + ',';
                }
                foreach (var keyValueFeacher in keyValueFeachers)
                {
                    headers += keyValueFeacher + ',';
                }
                foreach (var listWordFeatcher in listWordFeatchers)
                {
                    headers += String.Join("|", listWordFeatcher) + ',';
                }
                if (headers[headers.Length - 1] == ',')
                    headers = headers.Substring(0, headers.Length - 2);
                resaultList.Add(headers);
            }
        }

        private static string BasicInfo(string file, string temporalResault, string fileNameLower, bool isCsvInput)
        {
            if (!isCsvInput)
            {
                temporalResault += "file address: " + file + "\n";
                temporalResault += "username: " + fileNameLower.Substring(fileNameLower.IndexOf('u') + 2).Replace(".xml", "") + "\n";
            }
            else
            {
                temporalResault += file + ",";
                temporalResault += fileNameLower.Substring(fileNameLower.IndexOf('u') + 2).Replace(".xml", "") + ",";
            }
            return temporalResault;
        }

        private static string CreatePading(string temporalResault, bool isCsvInput)
        {
            if (!isCsvInput)
            {
                temporalResault += "###############################################" + "\n";
                temporalResault += "..............................................." + "\n";
                temporalResault += "###############################################" + "\n";
                temporalResault += "\n";
            }
            return temporalResault;
        }

        private static void Input1(out List<string> countableFeachers, out List<string> keyValueFeachers, out List<List<string>> listWordFeatchers)
        {
            countableFeachers = new List<string>() { "door lock", "win", "bounce", "execute", "solved q", "solved failed", "hack", "pause", "show tuterial", "go directly", "surending clicked", "surending become available" }.ConvertAll(d => d.ToLower()); ;
            keyValueFeachers = new List<string>() { "Hours", "Difficulty", "PlayFasility" }.ConvertAll(d => d.ToLower());
            listWordFeatchers = new List<List<string>>();
        }

        private static void Input2(out List<string> countableFeachers, out List<string> keyValueFeachers, out List<List<string>> listWordFeatchers)
        {
            countableFeachers = new List<string>() { "surending clicked", "surending become available", "next quize clicked", "prv quize clicked",
                "galexyShooter", "matching", "help button clicked","open challange menue", "target selected", "source and target match", "source and target do not match",
                "shoot location", "galexy shooter game entered", "life is lost location", "power up catched", "create power up",
                "create astroid:^objectType^GoldenAstroid","object go out :^objectType^GoldenAstroid","create astroid:^objectType^MetalAstroid","create astroid","create enemy",
                "restartBtn","btn restart","challange passed","get out of negetive","get out of posetive","get out of",
                "LifeChallangeIn2MDone","ShipKillingChallangeDone","AstroidKillingChallangeDone","GoldChallangeDone","NotUseLaserChallangeDone","ScoreChallangeDone" }.ConvertAll(d => d.ToLower());
            //countableFeachers = new List<string>();
            keyValueFeachers = new List<string>();

            listWordFeatchers = new List<List<string>>() {
                new List<string>() { "Colide gold", "with player" }.ConvertAll(d => d.ToLower()),
                new List<string>() { "Colide gold", "pLaser" }.ConvertAll(d => d.ToLower()),
                new List<string>() { "Colide astroid", "with player" }.ConvertAll(d => d.ToLower()),
                new List<string>() { "Colide astroid", "pLaser" }.ConvertAll(d => d.ToLower()),
                new List<string>() { "Colide Enemy", "with player" }.ConvertAll(d => d.ToLower()),
                new List<string>() { "Colide Enemy", "pLaser" }.ConvertAll(d => d.ToLower()),
                new List<string>() { "object go out", "Astroid" }.ConvertAll(d => d.ToLower()),
                new List<string>() { "object go out", "Enemy" }.ConvertAll(d => d.ToLower()),
                new List<string>() { "object go out", "Powerup" }.ConvertAll(d => d.ToLower())};
        }

        private static void Input3(out List<string> countableFeachers)
        {
            countableFeachers = new List<string>() {
            "preTuterial DoorLock1 to preTuterial Matching logs number" ,
            "preTuterial GalexyShooter to preTuterial DoorLock2 logs number" ,
            "preTuterial DoorLock2 to preTuterial Hack logs number" ,
            "preTuterial Hack to preTuterial Matching Finish Page logs number" ,
            "preTuterial DoorLock1 to preTuterial Matching duration" ,
            "preTuterial GalexyShooter to preTuterial DoorLock2 duration" ,
            "preTuterial DoorLock2 to preTuterial Hack duration" ,
            "preTuterial Hack to preTuterial Matching Finish Page duration" }.ConvertAll(d => d.ToLower());
        }
    }
    public enum MethoudToRun
    {
        FileSize,
        Main,
        MoveLRCalculator,
        TimeCalculator,
        MaxScoreFinder,
        RecordBasedFutureExtractor
    }
}
