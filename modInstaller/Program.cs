// @author: 4c65736975, All Rights Reserved
// @version: 1.0.0.0, 01/03/2023
// @filename: Program.cs

using System.IO.Compression;
using System.Xml;

namespace modInstaller
{
    internal class Program
    {
        private static bool isSuccess = false;
        public static void Main(string[] args)
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\My Games\FarmingSimulator2022\";

            bool isModDescFound = getModDescIsInDirectory();

            if (!isModDescFound)
            {
                Thread.Sleep(3000);

                return;
            }

            if (File.Exists(path + "gameSettings.xml"))
            {
                Console.WriteLine("Found game settings ! Gathering mod directory path information...");

                installMod(path);
            }
            else
            {
                Console.WriteLine("Couldn't find game settings ! Aborting installing with error !");
            }

            AppDomain.CurrentDomain.ProcessExit += (s, e) => {
                if (isSuccess)
                {
                    Directory.Delete(Environment.CurrentDirectory + @"\temp", true);
                }
            };
        }
        private static void installMod(string path)
        {
            string modDirectoryPath = getModsInstallDirectory(path);

            if (Directory.Exists(modDirectoryPath))
            {
                string currentDirectory = Environment.CurrentDirectory;
                string currentDirectoryName = Path.GetFileName(currentDirectory) ?? "mod";
                string[] filesToInstall = { "modDesc.xml", getModIconFilename(currentDirectory) };
                string[] directoriesToInstall = { "src", "i18n", "data" };

                if (!Directory.Exists(currentDirectory + @"\temp"))
                {
                    Directory.CreateDirectory(currentDirectory + @"\temp");
                }

                string[] files = Directory.GetFiles(currentDirectory);
                string[] directories = Directory.GetDirectories(currentDirectory);

                if (Directory.Exists(currentDirectory + @"\temp"))
                {
                    foreach (string file in files)
                    {
                        string filename = Path.GetFileName(file);

                        foreach (string fileToInstall in filesToInstall)
                        {
                            string installFilename = Path.GetFileName(fileToInstall);

                            if (installFilename == filename)
                            {
                                File.Move(currentDirectory + @$"\{installFilename}", currentDirectory + @$"\temp\{installFilename}");
                            }
                            else
                            {
                                deleteUnnecessaryFile(file);
                            }
                        }
                    }

                    foreach (string directory in directories)
                    {
                        string filename = Path.GetFileName(directory);

                        foreach (string directoryToInstall in directoriesToInstall)
                        {
                            string installFilename = Path.GetFileName(directoryToInstall);

                            if (installFilename == filename)
                            {
                                Directory.Move(currentDirectory + @$"\{installFilename}", currentDirectory + @$"\temp\{installFilename}");
                            }
                            else
                            {
                                deleteUnnecessaryFile(directory);
                            }
                        }
                    }
                }

                try
                {
                    ZipFile.CreateFromDirectory(currentDirectory + @"/temp", $"FS22_{currentDirectoryName}_dev.zip");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{ex.Message}");
                }

                if (File.Exists(currentDirectory + @$"\FS22_{currentDirectoryName}_dev.zip"))
                {
                    File.Move(currentDirectory + @$"\FS22_{currentDirectoryName}_dev.zip", modDirectoryPath + @$"\FS22_{currentDirectoryName}_dev.zip", true);
                }

                Console.Write($"Mod is successfully installed ! Program will close automatically in  ");

                for (int i = 3; i >= 1; i--)
                {
                    Console.SetCursorPosition(Console.CursorLeft - 1, Console.CursorTop);
                    Console.Write($"{i}");
                    Thread.Sleep(1000);
                }

                isSuccess = true;
            }
            else
            {
                Console.WriteLine($"Path = '{modDirectoryPath}' doesn't exists ! Aborting installing with error !");
                Console.WriteLine("Type any key to exit.");

                Console.ReadKey();
            }
        }
        private static void deleteUnnecessaryFile(string file)
        {
            string[] toDelete = { "screenshots", "LICENSE", "README.md", ".gitignore", ".github" };

            if (file != null && file != "")
            {
                foreach (string s in toDelete)
                {
                    if (s == Path.GetFileName(file))
                    {
                        try
                        {
                            File.Delete(file);
                        }
                        catch (Exception ex)
                        {
                            try
                            {
                                Directory.Delete(file, true);
                            }
                            catch (Exception ex2)
                            {
                                Console.WriteLine($"{ex.Message}");
                                Console.WriteLine($"{ex2.Message}");
                            }
                        }
                    }
                }
            }
        }
        private static string getModIconFilename(string path)
        {
            string modDesc = path + "/modDesc.xml";
            string icon = "icon.dds";

            using (XmlReader reader = XmlReader.Create(modDesc))
            {
                reader.ReadToFollowing("iconFilename");

                icon = reader.ReadElementContentAsString("iconFilename", "");

                reader.Close();
            }

            icon = icon.Replace(".png", ".dds");

            return icon;
        }
        private static string getModsInstallDirectory(string path)
        {
            string modDirectoryPath = path + "mods";

            using (XmlReader reader = XmlReader.Create(path + "gameSettings.xml"))
            {
                reader.ReadToDescendant("modsDirectoryOverride");
                reader.MoveToAttribute("active");

                bool isOverwritten = reader.ReadContentAsBoolean();

                if (isOverwritten)
                {
                    reader.MoveToAttribute("directory");

                    modDirectoryPath = reader.ReadContentAsString();
                }

                reader.Close();
            }

            return modDirectoryPath;
        }
        private static bool getModDescIsInDirectory()
        {
            string currentDirectory = Environment.CurrentDirectory;

            bool isModDescFound = false;

            if (File.Exists(currentDirectory + @"\modDesc.xml"))
            {
                isModDescFound = true;
            }
            else
            {
                Console.WriteLine("Couldn't find 'modDesc.xml' in current directory !");
            }

            return isModDescFound;
        }
    }
}