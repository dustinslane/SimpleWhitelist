using CitizenFX.Core;
using CitizenFX.Core.Native;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace SimpleWhitelist
{
    /// <summary>
    /// A super simple steamid-based whitelist class that you can use to keep random people off of your server.
    /// This is great for when you are running a small server for your friends or just need a server to test something on, and you don't want
    /// outsiders on it. There are other scripts/classes out there that basically do the same, but all of them require some form 
    /// of a database or fiddling with JSON files.
    /// 
    /// This resource lets you add/remove people through RCON so you can add/remove on the fly, and saves it in plain text on your server.
    /// 
    /// Written by Dustin Slane https://github.com/dustinslane/
    /// Project can be found on https://github.com/dustinslane/SimpleWhitelist
    /// Requires FXServer 1145 or later.
    /// Version 1.0.0
    /// </summary>
    public class SimpleWhitelist : BaseScript
    {
        /// <summary>
        /// Whitelist
        /// 
        /// Contains the steam id's allowed on the server
        /// </summary>
        private List<string> _whitelist;

        /// <summary>
        /// Name of the whitelist file the whitelist is saved to.
        /// </summary>
        private const string FileName = "SimpleWhitelist.txt";

        public SimpleWhitelist()
        {
            InitialiseList();

            EventHandlers["rconCommand"] += new Action<string, List<object>>(OnRconCommand);
            EventHandlers["playerConnecting"] += new Action<Player, string, dynamic, dynamic>(OnPlayerConnecting);
        }

        /// <summary>
        /// Initialise the whitelist. 
        /// Checks to see if the file exists, then loads it.
        /// </summary>
        private async void InitialiseList()
        {
            Log("Loading whitelist...");
            try
            {
                _whitelist = new List<string>();

                // If the file does not exist, or there is a problem loading it, create a new file.
                if (!File.Exists(FileName) || !await LoadWhitelist())
                {
                    SaveWhitelist();
                }
            }
            catch (Exception e)
            {
                Log(e);
            }
        }

        #region EventHandlers

        /// <summary>
        /// Handle an RCON command.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="arguments"></param>
        private void OnRconCommand(string command, List<object> arguments)
        {
            try
            {
                int argCount = arguments?.Count ?? 0;
                switch (command.ToLower())
                {
                    case "whitelist.add":
                        {
                            if (argCount == 0)
                            {
                                Log("Usage: whitelist.add [steam id]");
                                break;
                            }
                            var id = Convert.ToString(arguments[0]).ToLower().Trim();
                            if (_whitelist.Contains(id))
                            {
                                Log($"User {id} is already whitelisted!");
                                break;
                            }

                            _whitelist.Add(id);
                            SaveWhitelist();
                            Log($"Added {id} to the whitelist!");
                            break;
                        }

                    case "whitelist.remove":
                        {
                            if (arguments.Count == 0)
                            {
                                Log("Usage: whitelist.remove [steam id]");
                                break;
                            }

                            var id = Convert.ToString(arguments[0]).ToLower().Trim();
                            if (!_whitelist.Contains(id))
                            {
                                Log($"User {id} is not whitelisted!");
                                return;
                            }

                            _whitelist.Remove(id);
                            SaveWhitelist();
                            Log($"Removed {id} from the whitelist!");
                            break;
                        }

                    case "whitelist.list":
                        {
                            Log("SimpleWhitelist - Whitelisted players:");
                            _whitelist.ForEach(Log);
                            break;
                        }


                    case "whitelist":
                        {
                            Log("Usage: whitelist.add [steam id], whitelist.remove [steamid], whitelist.list");
                            break;
                        }

                    default:
                        {
                            return;
                        }
                }

                API.CancelEvent();
            }
            catch (Exception e)
            {
                Log(e);
            }
        }

        /// <summary>
        /// Handle a connecting player
        /// </summary>
        /// <param name="player">Player</param>
        /// <param name="playerName">Name of the player</param>
        /// <param name="kick">Kick method</param>
        /// <param name="deferrals">Deferrals</param>
        private void OnPlayerConnecting([FromSource]Player player, string playerName, dynamic kick, dynamic deferrals)
        {
            try
            {
                deferrals.defer();
                var steam = player.Identifiers["steam"];
                if (string.IsNullOrEmpty(steam))
                {
                    kick("Could not find your steam connection. Please start steam and relaunch.");
                    API.CancelEvent();
                    return;
                }

                Log($"Player {playerName} with steam id {steam} is joining...");
                deferrals.update($"Hello {playerName}. Please hold while we check your ticket...");

                if (!_whitelist.Contains(steam.ToLower()))
                {
                    kick("You are not whitelisted. Please contact the server owner to get whitelisted.");
                    Log($"Kicked player {playerName} for not being whitelisted");
                    API.CancelEvent();
                    return;
                }

                Log($"Player {playerName} is whitelisted, loading in!");
                deferrals.done();
            }
            catch (Exception e)
            {
                Log(e);
            }
        }

        #endregion

        #region Internal

        /// <summary>
        /// Load the whitelist from file
        /// </summary>
        /// <returns>True if load was successful, False if load threw an exception</returns>
        private async Task<bool> LoadWhitelist()
        {
            try
            {
                string content = await ReadAsync(FileName);
                _whitelist.Clear();
                _whitelist.AddRange(content.Split('\n'));
                Log($"Successfully loaded {_whitelist.Count} whitelist entries");
                return true;
            }
            catch (Exception ex)
            {
                Log(ex);
                return false;
            }
        }

        /// <summary>
        /// Save the whitelist to file
        /// </summary>
        private async void SaveWhitelist()
        {
            try
            {
                await WriteAsync(FileName, string.Join("\n", _whitelist));

                Log($"Successfully wrote {_whitelist.Count} entries to the whitelist file");
            }
            catch (Exception e)
            {
                Log(e);
            }
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Log something to the console
        /// </summary>
        /// <param name="log"></param>
        private void Log(string log)
        {
            Debug.WriteLine("SimpleWhitelisting [Info] " + log);
        }

        /// <summary>
        /// Log an exception to the console
        /// </summary>
        /// <param name="e"></param>
        private void Log(Exception e)
        {
            Debug.WriteLine("SimpleWhitelisting [Error] " + e.Message);
            Debug.WriteLine("SimpleWhitelisting [Error] " + e.StackTrace);
        }

        /// <summary>
        /// Write to a file async
        /// </summary>
        /// <param name="file">file to write to</param>
        /// <param name="text">contents to write</param>
        private async Task WriteAsync(string file, string text)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(file, false))
                {
                    await writer.WriteAsync(text);
                }
            }
            catch (Exception e)
            {
                Log(e);
            }

        }

        /// <summary>
        /// Read file async
        /// </summary>
        /// <param name="file">file name</param>
        /// <returns>file contents</returns>
        private async Task<string> ReadAsync(string file)
        {
            try
            {
                string value = "";
                using (StreamReader reader = new StreamReader(file))
                {
                    value = await reader.ReadToEndAsync();
                }

                return value;
            }
            catch (Exception e)
            {
                Log(e);
                return "";
            }
        }

        #endregion
    }
}
