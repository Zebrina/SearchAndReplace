using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchAndReplace {
    static class Program {
        static int Main(string[] args) {
            string find = null;
            string replaceWith = null;
            string directory = ".";
            bool bCaseSensitive = false;
            bool bRecursive = false;

            for (int i = 0; i < args.Length; ++i) {
                switch (args[i]) {
                    case "/?":
                    case "/h":
                    case "/H":
                        PrintUsage();
                        return 0;

                    case "/c":
                    case "/C":
                        bCaseSensitive = true;
                        break;

                    case "/r":
                    case "/R":
                        bRecursive = true;
                        break;

                    default:
                        if (String.Equals(args[i], "help", StringComparison.OrdinalIgnoreCase)) {
                            PrintUsage();
                            return 0;
                        }
                        else if (find == null) {
                            find = args[i];
                        }
                        else if (replaceWith == null) {
                            replaceWith = args[i];
                        }
                        else if (directory == ".") {
                            directory = args[i];
                        }
                        else {
                            Console.WriteLine("Unrecognised command line argument: " + args[i]);
                            PrintUsage();
                            return 1;
                        }
                        break;
                }
            }

            if (!String.IsNullOrEmpty(find) && replaceWith != null && Directory.Exists(directory)) {
                FindAndReplace(directory, find, replaceWith, bRecursive, bCaseSensitive);
                return 0;
            }

            PrintUsage();

            return 2;
        }

        static void PrintUsage() {
            Console.WriteLine("Search and replace text in file names.\n");
            Console.WriteLine("Usage:\nsnr [/C | /R] find replace [OPTIONAL][drive:][path]dirname\n");
            Console.WriteLine("  find\t\tSpecifies the string to search for.");
            Console.WriteLine("  replace\tSpecifies the string to replace find with.");
            Console.WriteLine("  dirname\tSpecifies the directory where file names will be renamed. Defaults to current working directory if ommited.");
            Console.WriteLine("  /C\t\tForce case sensitivity.");
            Console.WriteLine("  /R\t\tRecursively rename files in subdirectories.\n");
            Console.WriteLine("Print help:\nsnr [/? | /H | help]\n");
        }

        static void FindAndReplace(string directoryPath, string find, string replaceWith, bool includeSubdirectories, bool caseSensitive) {
            StringComparison strcmp = caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

            foreach (string file in Directory.EnumerateFiles(directoryPath)) {
                FileInfo fileInfo = new FileInfo(file);
                if (fileInfo.Exists) {
                    string fileName = fileInfo.Name;
                    string newFileName;
                    if (caseSensitive) {
                        newFileName = fileName.Replace(find, replaceWith);
                    }
                    else {
                        newFileName = fileName;

                        int searchIndex = 0;
                        while (true) {
                            int index = newFileName.IndexOf(find, searchIndex, StringComparison.OrdinalIgnoreCase);
                            if (index < 0) {
                                break;
                            }

                            newFileName = newFileName.Remove(index, find.Length);
                            newFileName = newFileName.Insert(index, replaceWith);

                            searchIndex = index + replaceWith.Length;
                        }
                    }
                    if (!String.Equals(fileName, newFileName, StringComparison.OrdinalIgnoreCase)) {
                        try {
                            File.Move(fileInfo.FullName, fileInfo.DirectoryName + "\\" + newFileName);
                            Console.WriteLine("'" + fileInfo.FullName + "' successfully renamed to '" + newFileName + "'");
                        }
                        catch (Exception e) {
                            Console.WriteLine("Something went wrong: " + e.ToString());
                        }
                    }
                }
            }

            if (includeSubdirectories) {
                foreach (string path in Directory.EnumerateDirectories(directoryPath)) {
                    FindAndReplace(path, find, replaceWith, includeSubdirectories, caseSensitive);
                }
            }
        }
    }
}
