﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
//using VideoLibrary;
using DuckDuckGo.Net;
using OxfordDictionariesAPI;
using HtmlAgilityPack;
using System.Drawing;
using System.Drawing.Imaging;

namespace ForkBot
{
    public class Commands : ModuleBase
    {
        Random rdm = new Random();

        [Command("mhelp"), Summary("Displays Moderator commands and descriptions"), RequireUserPermission(GuildPermission.BanMembers)]
        public async Task MHelp()
        {
            JEmbed emb = new JEmbed();
            emb.Author.Name = "ForkBot Commands";
            emb.ThumbnailUrl = Context.User.AvatarId;
            if (Context.Guild != null) emb.ColorStripe = Functions.GetColor(Context.User);
            else emb.ColorStripe = Constants.Colours.DEFAULT_COLOUR;
            foreach (CommandInfo command in Bot.commands.Commands)
            {
                if (command.Summary != null && command.Summary.StartsWith("[MOD]"))
                {
                    emb.Fields.Add(new JEmbedField(x =>
                    {
                        string header = command.Name;
                        foreach (String alias in command.Aliases) if (alias != command.Name) header += " (;" + alias + ") ";
                        foreach (ParameterInfo parameter in command.Parameters) header += " [" + parameter.Name + "]";
                        x.Header = header;
                        x.Text = command.Summary;
                    }));
                }
            }
            await Context.Channel.SendMessageAsync("", embed: emb.Build());
        }

        [Command("help"), Summary("Displays commands and descriptions.")]
        public async Task Help()
        {
            JEmbed emb = new JEmbed();
            emb.Author.Name = "ForkBot Commands";
            emb.ThumbnailUrl = Context.User.AvatarId;
            if (Context.Guild != null) emb.ColorStripe = Functions.GetColor(Context.User);
            else emb.ColorStripe = Constants.Colours.DEFAULT_COLOUR;
            foreach (CommandInfo command in Bot.commands.Commands)
            {
                if (command.Summary != null && !command.Summary.StartsWith("[MOD]")) {
                    emb.Fields.Add(new JEmbedField(x =>
                    {
                        string header = command.Name;
                        foreach (String alias in command.Aliases) if (alias != command.Name) header += " (;" + alias + ") ";
                        foreach (ParameterInfo parameter in command.Parameters) header += " [" + parameter.Name + "]";
                        x.Header = header;
                        x.Text = command.Summary;
                    }));
                }
            }
            await Context.Channel.SendMessageAsync("", embed: emb.Build());
        }

        /*[Command("play"), Summary("Play a song from Youtube.")]
        public async Task Play(string song)
        {

            var youTube = YouTube.Default; // starting point for YouTube actions
            var video = await youTube.GetVideoAsync(song); // gets a Video object with info about the video
            File.WriteAllBytes(@"Video\" + video.FullName, video.GetBytes());
            
        }*/

        [Command("hangman"), Summary("Play a game of Hangman with the bot."), Alias(new string[] { "hm" })]
        public async Task HangMan()
        {
            if (!Var.hangman)
            {
                var wordList = File.ReadAllLines("Files/wordlist.txt");
                Var.hmWord = wordList[(rdm.Next(wordList.Count()))].ToLower();
                Var.hangman = true;
                Var.hmCount = 0;
                Var.hmErrors = 0;
                Var.guessedChars.Clear();
                await HangMan("");
            }
            else
            {
                await Context.Channel.SendMessageAsync("There is already a game of HangMan running.");
            }
        }

        [Command("hangman"), Alias(new string[] { "hm" })]
        public async Task HangMan(string guess)
        {
            if (Var.hangman)
            {
                guess = guess.ToLower();
                if (guess != "" && Var.guessedChars.Contains(guess[0]) && guess.Count() == 1) await Context.Channel.SendMessageAsync("You've already guessed " + Char.ToUpper(guess[0]));
                else
                {
                    if (guess.Count() == 1 && !Var.guessedChars.Contains(guess[0])) Var.guessedChars.Add(guess[0]);
                    if (guess != "" && ((!Var.hmWord.Contains(guess[0]) && guess.Count() == 1) || (Var.hmWord != guess && guess.Count() > 1))) Var.hmErrors++;


                    string[] hang = {
            "       ______   " ,    //0
            "      /      \\  " ,   //1
            "     |          " ,    //2
            "     |          " ,    //3
            "     |          " ,    //4
            "     |          " ,    //5
            "     |          " ,    //6
            "_____|_____     " };   //7

                    for (int i = 0; i < Var.hmWord.Count(); i++)
                    {
                        if (Var.guessedChars.Contains(Var.hmWord[i])) hang[6] += Char.ToUpper(Convert.ToChar(Var.hmWord[i])) + " ";
                        else hang[6] += "_ ";
                    }

                    for (int i = 0; i < Var.hmErrors; i++)
                    {
                        if (i == 0)
                        {
                            var line = hang[2].ToCharArray();
                            line[13] = 'O';
                            hang[2] = new string(line);
                        }
                        if (i == 1)
                        {
                            var line = hang[3].ToCharArray();
                            line[13] = '|';
                            hang[3] = new string(line);
                        }
                        if (i == 2)
                        {
                            var line = hang[4].ToCharArray();
                            line[12] = '/';
                            hang[4] = new string(line);
                        }
                        if (i == 3)
                        {
                            var line = hang[4].ToCharArray();
                            line[14] = '\\';
                            hang[4] = new string(line);
                        }
                        if (i == 4)
                        {
                            var line = hang[3].ToCharArray();
                            line[12] = '/';
                            hang[3] = new string(line);
                        }
                        if (i == 5)
                        {
                            var line = hang[3].ToCharArray();
                            line[14] = '\\';
                            hang[3] = new string(line);
                        }
                    }

                    if (!hang[6].Contains("_") || Var.hmWord == guess) //win
                    {
                        await Context.Channel.SendMessageAsync("You did it!");
                        Var.hangman = false;

                        var u = Functions.GetUser(Context.User);
                        u.Coins += 10;
                        Functions.SaveUsers();
                    }

                    if (Var.hmErrors == 6)
                    {
                        await Context.Channel.SendMessageAsync("You lose! The word was: " + Var.hmWord);
                        Var.hangman = false;
                    }

                    string msg = "```\n";
                    foreach (String s in hang) msg += s + "\n";
                    msg += "```";
                    if (Var.hangman)
                    {
                        msg += "Guessed letters: ";
                        foreach (char c in Var.guessedChars) msg += char.ToUpper(c) + " ";
                        msg += "\nUse `;hangman [guess]` to guess a character or the entire word.";

                    }
                    await Context.Channel.SendMessageAsync(msg);
                }
            }
            else
            {
                await HangMan();
                await HangMan(guess);
            }
        }

        [Command("profile"), Summary("View your or another users profile.")]
        public async Task Profile(IUser user)
        {
            var u = Functions.GetUser(user);

            var emb = new JEmbed();
            emb.Author.Name = user.Username;
            emb.Author.IconUrl = user.GetAvatarUrl();

            emb.ColorStripe = Functions.GetColor(user);

            emb.Fields.Add(new JEmbedField(x =>
            {
                x.Header = "Coins:";
                x.Text = Convert.ToString(u.Coins);
                x.Inline = true;
            }));

            emb.Fields.Add(new JEmbedField(x =>
            {
                x.Header = "Roles:";
                string text = "";
                foreach (ulong id in (Context.User as IGuildUser).RoleIds)
                {
                    text += Context.Guild.GetRole(id).Name + "\n";
                }

                x.Text = Convert.ToString(text);
                x.Inline = true;
            }));

            emb.Fields.Add(new JEmbedField(x =>
            {
                x.Header = "Inventory:";
                string text = "";
                foreach (string item in u.Items)
                {
                    text += item + ", ";
                }
                x.Text = Convert.ToString(text);
            }));

            await Context.Channel.SendMessageAsync("", embed: emb.Build());
        }

        [Command("profile")]
        public async Task Profile() { await Profile(Context.User); }

        [Command("listusers")]
        public async Task Listusers()
        {
            string msg = "```\n";
            foreach (User u in Bot.users)
            {
                msg += u.Username() + " " + u.ID + ". Coins: " + u.Coins + "\n";
            }
            msg += "```";
            await Context.Channel.SendMessageAsync(msg);
        }

        [Command("present"), Summary("Get a cool gift!")]
        public async Task Present()
        {
            if (!Var.presentWaiting)
            {
                Var.presentNum = rdm.Next(10);
                await Context.Channel.SendMessageAsync($"A present appears! :gift: Press {Var.presentNum} to open it!");
                Var.presentWaiting = true;
                Var.replacing = false;
                Var.replaceable = true;
            }
        }

        [Command("whatis"), Alias(new string[] { "wi" }), Summary("Don't know what something is? Find out!")]
        public async Task WhatIs([Remainder]string thing)
        {
            var results = new Search().Query(thing, "ForkBot");
            QueryResult result = null;
            if (results.Abstract == "" && results.RelatedTopics.Count > 0) result = results.RelatedTopics[0];

            if (result != null)
            {
                JEmbed emb = new JEmbed();
                emb.Title = thing;
                emb.Description = result.Text;
                emb.ImageUrl = result.Icon.Url;
                await Context.Channel.SendMessageAsync("", embed: emb.Build());
            }
            else await Context.Channel.SendMessageAsync("No results found!");

        }

        [Command("define"), Alias(new string[] { "def" }), Summary("Returns the definiton for the inputted word.")]
        public async Task Define([Remainder]string word)
        {
            OxfordDictionaryClient client = new OxfordDictionaryClient("45278ea9", "c4dcdf7c03df65ac5791b67874d956ce");
            var result = await client.SearchEntries(word, CancellationToken.None);
            if (result != null)
            {
                var senses = result.Results[0].LexicalEntries[0].Entries[0].Senses[0];

                JEmbed emb = new JEmbed();
                emb.Title = Func.ToTitleCase(word);
                emb.Description = Char.ToUpper(senses.Definitions[0][0]) + senses.Definitions[0].Substring(1) + ".";
                emb.ColorStripe = Constants.Colours.YORK_RED;
                if (senses.Examples != null)
                {
                    emb.Fields.Add(new JEmbedField(x =>
                    {
                        x.Header = "Examples:";
                        string text = "";
                        foreach (OxfordDictionariesAPI.Models.Example eg in senses.Examples)
                        {
                            text += $"\"{Char.ToUpper(eg.Text[0]) + eg.Text.Substring(1)}.\"\n";
                        }
                        x.Text = text;
                    }));
                }
                await Context.Channel.SendMessageAsync("", embed: emb.Build());
            }
            else await Context.Channel.SendMessageAsync($"Could not find definition for: {word}.");
        }

        [Command("professor"), Alias(new string[] { "prof", "rmp" }), Summary("Check out a professors rating from RateMyProfessors.com!")]
        public async Task Professor([Remainder]string name)
        {
            HtmlWeb web = new HtmlWeb();

            string link = "http://www.ratemyprofessors.com/search.jsp?query=" + name.Replace(" ", "%20");
            var page = web.Load(link);
            var node = page.DocumentNode.SelectSingleNode("//*[@id=\"searchResultsBox\"]/div[2]/ul/li[1]");
            if (node != null)
            {
                string tid = Functions.GetTID(node.InnerHtml);

                var newLink = "http://www.ratemyprofessors.com/ShowRatings.jsp?tid=" + tid;
                page = web.Load(newLink);

                var rating = page.DocumentNode.SelectSingleNode("//*[@id=\"mainContent\"]/div[1]/div[3]/div[1]/div/div[1]/div/div/div").InnerText;
                var takeAgain = page.DocumentNode.SelectSingleNode("//*[@id=\"mainContent\"]/div[1]/div[3]/div[1]/div/div[2]/div[1]/div").InnerText;
                var difficulty = page.DocumentNode.SelectSingleNode("//*[@id=\"mainContent\"]/div[1]/div[3]/div[1]/div/div[2]/div[2]/div").InnerText;
                var imageNode = page.DocumentNode.SelectSingleNode("//*[@id=\"mainContent\"]/div[1]/div[1]/div[2]/div[1]/div[1]/img");
                var titleText = page.DocumentNode.SelectSingleNode("/html/head/title").InnerText;
                string profName = titleText.Split(' ')[0] + " " + titleText.Split(' ')[1];

                var tagsNode = page.DocumentNode.SelectSingleNode("//*[@id=\"mainContent\"]/div[1]/div[3]/div[2]/div[2]");
                List<string> tags = new List<string>();
                for (int i = 0; i < tagsNode.ChildNodes.Count(); i++)
                {
                    if (tagsNode.ChildNodes[i].Name == "span") tags.Add(tagsNode.ChildNodes[i].InnerText);
                }

                var hotness = page.DocumentNode.SelectSingleNode("//*[@id=\"mainContent\"]/div[1]/div[3]/div[1]/div/div[2]/div[3]/div/figure/img").Attributes[0].Value;
                var hotnessIMG = "http://www.ratemyprofessors.com" + hotness;
                string imageURL = null;
                if (imageNode != null) imageURL = imageNode.Attributes[0].Value;

                var commentsNode = page.DocumentNode.SelectSingleNode("/ html[1] / body[1] / div[2] / div[4] / div[3] / div[1] / div[7] / table[1]");

                List<string> comments = new List<string>();
                for (int i = 3; i < commentsNode.ChildNodes.Count(); i++)
                {
                    if (commentsNode.ChildNodes[i].Name == "tr" && commentsNode.ChildNodes[i].Attributes.Count() == 2)
                    {
                        comments.Add(commentsNode.ChildNodes[i].ChildNodes[5].ChildNodes[3].InnerText.Replace("\r\n               ", "").Replace("/", " "));
                    }
                }
                List<string> words = new List<string>();
                List<int> counts = new List<int>();

                foreach (string comment in comments)
                {
                    foreach (string dWord in comment.Split(' '))
                    {
                        string word = dWord.ToLower().Replace(".", "").Replace(",", "").Replace("'", "").Replace("(", "").Replace(")", "").Replace("!", "").Replace("?", "");
                        if (word != "")
                        {
                            if (words.Contains(word)) counts[words.IndexOf(word)]++;
                            else
                            {
                                words.Add(word);
                                counts.Add(1);
                            }
                        }
                    }
                }

                List<string> OrderedWords = new List<string>();
                for (int i = counts.Max(); i >= 0; i--)
                {
                    for (int c = 0; c < counts.Count(); c++)
                    {
                        if (counts[c] == i)
                        {
                            OrderedWords.Add(words[counts.IndexOf(counts[c])]);
                            break;
                        }
                    }
                }
                string[] commonWords = { "youll", "if", "an", "not", "it", "as", "is", "in", "for", "but", "so", "on", "he", "the", "and", "to", "a", "are", "his", "she", "her", "you", "of", "hes", "shes", "prof", profName.ToLower().Split(' ')[0], profName.ToLower().Split(' ')[1] };
                foreach (string wrd in commonWords) OrderedWords.Remove(wrd);

                JEmbed emb = new JEmbed();

                emb.Title = profName;
                if (imageURL != null) emb.ImageUrl = imageURL;
                emb.ThumbnailUrl = hotnessIMG;
                emb.Fields.Add(new JEmbedField(x =>
                {
                    x.Header = "Rating:";
                    x.Text = rating;
                    x.Inline = true;
                }));

                emb.Fields.Add(new JEmbedField(x =>
                {
                    x.Header = "Difficulty:";
                    x.Text = difficulty;
                    x.Inline = true;
                }));

                emb.Fields.Add(new JEmbedField(x =>
                {
                    x.Header = "Would take again?:";
                    x.Text = takeAgain;
                    x.Inline = true;
                }));

                emb.Fields.Add(new JEmbedField(x =>
                {
                    x.Header = "Top Tags:";
                    string text = "";
                    foreach (string s in tags)
                    {
                        text += s;
                    }
                    x.Text = text;
                    x.Inline = false;
                }));

                emb.Fields.Add(new JEmbedField(x =>
                {
                    x.Header = "Common Comments:";
                    string text = "";
                    foreach (string s in OrderedWords)
                    {
                        text += Func.ToTitleCase(s) + ", ";
                    }
                    text = text.Substring(0, text.Count() - 2);
                    x.Text = text;
                    x.Inline = false;
                }));

                emb.ColorStripe = Constants.Colours.YORK_RED;
                await Context.Channel.SendMessageAsync("", embed: emb.Build());
            }
            else
            {
                await Context.Channel.SendMessageAsync("Professor not found!");
            }
        }

        #region Item Commands
        [Command("roll")]
        public async Task Roll(int max)
        {
            if (Functions.GetUser(Context.User).Items.Contains("game_die"))
            {
                await Context.Channel.SendMessageAsync(":game_die: | " + Convert.ToString(rdm.Next(max) + 1));
            }
        }

        [Command("roll")]
        public async Task Roll() { await Roll(6); }

        [Command("8ball")]
        public async Task EightBall([Remainder] string question)
        {
            if (Functions.GetUser(Context.User).Items.Contains("8ball"))
            {
                string[] answers = { "Yes", "No", "Ask again later", "Cannot predict now", "Unlikely", "Chances good", "Likely", "Lol no", "If you believe" };
                await Context.Channel.SendMessageAsync(":8ball: | " + answers[rdm.Next(answers.Count())]);
            }
        }

        [Command("sell"), Summary("Sell items from your inventory.")]
        public async Task Sell(string item)
        {
            var u = Functions.GetUser(Context.User);
            if (u.Items.Contains(item))
            {
                u.Items.Remove(item);
                var items = Functions.GetItemList();
                int price = 0;
                foreach (string line in items)
                {
                    if (line.Split('|')[0] == item)
                    {
                        price = Convert.ToInt32(line.Split('|')[2]);
                        break;
                    }
                }

                if (rdm.Next(100) < 5)
                {
                    var rItems = Functions.GetRareItemList();
                    var rItemData = rItems[rdm.Next(rItems.Count())];
                    var itemName = rItemData.Split('|')[0].Replace('_', ' ');
                    var rMessage = rItemData.Split('|')[1];
                    
                    u.Items.Add(Var.present);
                    await Context.Channel.SendMessageAsync($"Wait... Something is happening.... Your {Func.ToTitleCase(item)} floats up into the air and glows... It becomes.. My GOD... IT BECOMES....\n\n" +
                                                           $"A {itemName}! :{itemName}: {rMessage}");
                }
                else
                {
                    u.Coins += price;
                    Functions.SaveUsers();
                    await Context.Channel.SendMessageAsync($"You successfully sold your {item} for {price} coins!");
                }
            }
            else await Context.Channel.SendMessageAsync($"You do not have an item called {item}!");
        }

        //Test me!
        [Command("trade"), Summary("Initiate a trade with another user!")]
        public async Task Trade(IUser user)
        {
            if (Functions.GetTrade(Context.User) == null && Functions.GetTrade(user) == null)
            {
                Var.trades.Add(new ItemTrade(Context.User, user));
                await Context.Channel.SendMessageAsync("", embed: new InfoEmbed("TRADE INVITE",
                    user.Mention + "! " + Context.User.Username + " has invited you to trade."
                    + " Type ';trade accept' to accept or ';trade deny' to deny!").Build());
            }
            else await Context.Channel.SendMessageAsync("Either you or the person you are attempting to trade with is already in a trade!"
                                                    +" If you accidentally left a trade going, use `;trade cancel` to cancel the trade.");
        }

        //Test me!
        [Command("trade")]
        public async Task Trade(string command, string param = "")
        {
            bool showMenu = false;
            var trade = Functions.GetTrade(Context.User);
            if (trade != null)
            {
                switch (command)
                {
                    case "accept":
                        if (!trade.Accepted) trade.Accept();
                        showMenu = true;
                        break;
                    case "deny":
                        if (!trade.Accepted)
                        {
                            await Context.Channel.SendMessageAsync("", embed: new InfoEmbed("TRADE DENIED", $"{trade.Starter().Mention}, {Context.User.Username} has denied the trade request.").Build());
                            Var.trades.Remove(trade);
                        }
                        break;
                    case "add":
                        if (param != "")
                        {
                            var success = trade.AddItem(Context.User, param);
                            if (success == false)
                            {
                                await Context.Channel.SendMessageAsync("Unable to add item. Are you sure you have it?");
                            }
                            else showMenu = true;
                        }
                        else await Context.Channel.SendMessageAsync("Please specify the item to add!");
                        break;
                    case "finish":
                        trade.Confirm(Context.User);
                        if (trade.IsCompleted()) Var.trades.Remove(trade);
                        else await Context.Channel.SendMessageAsync("Awaiting confirmation from other user.");
                        break;
                    case "cancel":
                        await Context.Channel.SendMessageAsync("", embed: new InfoEmbed("TRADE CANCELLED", $"{Context.User.Username} has cancelled the trade. All items have been returned.").Build());
                        Var.trades.Remove(trade);
                        break;
                }
            }
            else await Context.Channel.SendMessageAsync("You are not currently part of a trade.");

            if (showMenu) await Context.Channel.SendMessageAsync("", embed: trade.CreateMenu());

            if (trade.IsCompleted())
            {
                await Context.Channel.SendMessageAsync("", embed: new InfoEmbed("TRADE SUCCESSFUL", "The trade has been completed successfully.").Build());
                Var.trades.Remove(trade);
            }
        }
        #endregion

        //for viewing a tag
        [Command("tag"), Summary("Make or view a tag!")]
        public async Task Tag(string tag)
        {
            if (!File.Exists("Files/tags.txt")) File.Create("Files/tags.txt");
            string[] tags = File.ReadAllLines("Files/tags.txt");
            bool sent = false;

            foreach (string line in tags)
            {
                if (line.Split('|')[0] == tag)
                {
                    sent = true;
                    await Context.Channel.SendMessageAsync(line.Split('|')[1]);
                    break;
                }
            }

            if (!sent) await Context.Channel.SendMessageAsync("Tag not found!");

        }

        //for creating the tag
        [Command("tag")]
        public async Task Tag(string tag, [Remainder]string content)
        {
            if (!File.Exists("Files/tags.txt")) File.Create("Files/tags.txt");
            bool exists = false;
            string[] tags = File.ReadAllLines("Files/tags.txt");
            foreach (string line in tags)
            {
                if (line.Split('|')[0] == tag) { exists = true; break; }
            }

            if (!exists)
            {
                File.AppendAllText("Files/tags.txt", tag + "|" + content + "\n");
                await Context.Channel.SendMessageAsync("Tag created!");
            }
            else await Context.Channel.SendMessageAsync("Tag already exists!");
        }

        [Command("draw"), Summary("Gets ForkBot to draw you a lovely picture")]
        public async Task Draw(int count)
        {
            
            using (Bitmap bmp = new Bitmap(500, 500))
            {
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    for (int i = 0; i < count; i++)
                    {
                        var c = System.Drawing.Color.FromArgb(rdm.Next(256), rdm.Next(256), rdm.Next(256));
                        Brush b = new SolidBrush(c);
                        Pen p = new Pen(b);
                        int x = rdm.Next(500) + 1;
                        int y = rdm.Next(500) + 1;
                        int w = rdm.Next(500) + 1;
                        int h = rdm.Next(500) + 1;

                        int shape = rdm.Next(2);
                        switch (shape)
                        {
                            case 0:
                                g.FillEllipse(b, x, y, w, h);
                                break;
                            case 1:
                                g.FillRectangle(b, x, y, w, h);
                                break;
                        }
                    }

                    bmp.Save("Files/drawing.png");
                    await Context.Channel.SendFileAsync("Files/drawing.png");

                }
            }
        }

        #region Mod Commands

        [Command("ban"),RequireUserPermission(GuildPermission.BanMembers), Summary("[MOD] Bans the specified user. Can enter time in minutes that user is banned for, otherwise it is indefinite.")]
        public async Task Ban(IGuildUser u, int minutes = 0, [Remainder]string reason = null)
        {
            string rText = ".";
            if (reason != null) rText = $" for: \"{reason}\".";

            string tText = "";

            if (minutes != 0)
            {
                TimeSpan tSpan = new TimeSpan(0, minutes, 0);
                var unbanTime = DateTime.Now + tSpan;
                Var.leaveBanned.Add(u);
                Var.unbanTime.Add(unbanTime);
                tText = $"\nThey have been banned until {unbanTime}.";
            }

            InfoEmbed banEmb = new InfoEmbed("USER BAN", $"User: {u} has been banned{rText}.{tText}", Constants.Images.Ban);
            await Context.Guild.AddBanAsync(u, reason: reason);
            await Context.Channel.SendMessageAsync("", embed: banEmb.Build());
        }

        [Command("kick"), RequireUserPermission(GuildPermission.KickMembers), Summary("[MOD] Kicks the specified user.")]
        public async Task Kick(IUser u, [Remainder]string reason = null)
        {
            string rText = ".";
            if (reason != null) rText = $" for: \"{reason}\".";
            InfoEmbed kickEmb = new InfoEmbed("USER KICK", $"User: {u} has been kicked{rText}", Constants.Images.Kick);
            await (u as IGuildUser).KickAsync(reason);
            await Context.Channel.SendMessageAsync("", embed: kickEmb.Build());
        }

        [Command("move"), RequireUserPermission(GuildPermission.KickMembers), Summary("[MOD] Move people who are in the wrong channel to the correct channel.")]
        public async Task Move(IMessageChannel chan, params IUser[] users)
        {
            string mentionedUsers = "";
            foreach(IUser u in users)
            {
                mentionedUsers += u.Mention + ", ";
            }
            await chan.SendMessageAsync(mentionedUsers + " please keep discussions in their correct channel.");
            var channels = await Context.Guild.GetTextChannelsAsync();
            OverwritePermissions op = new OverwritePermissions(readMessages: PermValue.Deny);
            foreach (IGuildChannel c in channels)
            {
                foreach (IUser u in users)
                {
                    if (c != null && c.Id != chan.Id) await c.AddPermissionOverwriteAsync(u, op);
                }
            }
            Timers.mvChannel = chan;
            Timers.mvChannels = channels;
            Timers.mvUsers = users;

            Timers.mvTimer = new Timer(Timers.MoveTimer, null, 5000, Timeout.Infinite);
        }

        [Command("purge"), RequireUserPermission(GuildPermission.ManageMessages), Summary("[MOD] Delete [amount] messages")]
        public async Task Purge(int amount)
        {

            var messages = await Context.Channel.GetMessagesAsync(amount + 1).Flatten();
            await Context.Channel.DeleteMessagesAsync(messages);

            InfoEmbed ie = new InfoEmbed("PURGE", $"{amount} messages deleted.");
            await Context.Channel.SendMessageAsync("", embed: ie.Build());
        }
        #endregion
        


        #region Brady Commands

        [Command("remind")]
        public async Task Remind([Remainder] string reminder)
        {
            if (reminder != "")
            {
                if (Context.User.Id == Constants.Users.BRADY)
                {
                    File.AppendAllText("Files/reminders.txt", reminder + "\n");
                    await Context.Channel.SendMessageAsync("Added");
                }
            }
            else
            {
                string reminders = File.ReadAllText("Files/reminders.txt");
                await Context.Channel.SendMessageAsync(reminders);
            }
        }

        [Command("remind")]
        public async Task Remind() { await Remind(""); }

        [Command("givecoins"), Summary("Give the inputted user the specified amount of coins.")]
        public async Task Give(IUser user, int amount)
        {
            if (Context.User.Id == Constants.Users.BRADY)
            {
                User u = Functions.GetUser(user);
                u.Coins += amount;
                await Context.Channel.SendMessageAsync($"{user.Username} has successfully been given {amount} coins.");
            }
            else await Context.Channel.SendMessageAsync("Sorry, only Brady can use this right now.");
        }

        [Command("giveitem"), Summary("Give the inputted user the specified item.")]
        public async Task Give(IUser user, string item)
        {
            if (Context.User.Id == Constants.Users.BRADY)
            {
                User u = Functions.GetUser(user);
                u.Items.Add(item);
                await Context.Channel.SendMessageAsync($"{user.Username} has successfully been given: {item}.");
            }
            else await Context.Channel.SendMessageAsync("Sorry, only Brady can use this right now.");
        }
        #endregion
        //( ͡° ͜ʖ ͡°)
    }
}
