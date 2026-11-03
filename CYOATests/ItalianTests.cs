using System.Text.Json;

[TestClass]
public class ItalianTests
{
    private static Messages LoadItalian()
    {
        var messages = new Messages();
        messages.SetCurrentLanguage("Italian");
        messages.ReadDictionary();
        return messages;
    }

    [TestMethod]
    public void Italian_LoadsAllKeysAndPreservesFormatPlaceholders()
    {
        using var doc = JsonDocument.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "language_data.json")));
        var english = doc.RootElement.GetProperty("English").GetProperty("dictionary");
        var italian = doc.RootElement.GetProperty("Italian").GetProperty("dictionary");
        var messages = LoadItalian();
        Assert.AreEqual(english.EnumerateObject().Count(), italian.EnumerateObject().Count());
        foreach (var entry in english.EnumerateObject())
        {
            string translated = messages.GetMessage(entry.Name);
            Assert.AreNotEqual($"[{entry.Name}]", translated);
            Assert.IsFalse(string.IsNullOrWhiteSpace(translated), entry.Name);
            var pattern = @"\{\d+\}";
            CollectionAssert.AreEquivalent(
                System.Text.RegularExpressions.Regex.Matches(entry.Value.GetString()!, pattern).Select(m => m.Value).ToArray(),
                System.Text.RegularExpressions.Regex.Matches(translated, pattern).Select(m => m.Value).ToArray(), entry.Name);
            _ = string.Format(translated, "nome", "arma", "bersaglio");
        }
        Assert.AreEqual("Benvenuto nel gioco Scegli la tua avventura", messages.GetMessage("welcome"));
        Assert.AreEqual("L'avventura finisce. Arrivederci.", messages.GetMessage("goodbye"));
        Assert.AreEqual("[unknown_key]", messages.GetMessage("unknown_key"));
        messages.SetCurrentLanguage("English");
        messages.ReadDictionary();
        Assert.AreEqual("Welcome to Choose Your Adventure Game", messages.GetMessage("welcome"));
        messages.SetCurrentLanguage("Italian");
        messages.ReadDictionary();
        Assert.AreEqual("Italian", messages.CurrentLanguage);
        StringAssert.Contains(messages.GetMessage("welcome"), "Benvenuto");
    }

    [DataTestMethod]
    [DataRow("umano", "Human", "Umano")]
    [DataRow("elfo", "Elf", "Elfo")]
    [DataRow("nano", "Dwarf", "Nano")]
    [DataRow("halfling", "Halfling", "Halfling")]
    public void Italian_RacesNormalizeAndTranslate(string input, string canonical, string display)
    {
        var messages = LoadItalian();
        string padded = "  " + input.ToUpperInvariant() + "  ";
        Assert.IsTrue(messages.IsValidRace(padded));
        Assert.AreEqual(canonical, messages.NormalizeRace(padded));
        Assert.AreEqual(display, messages.TranslateRaceForDisplay(canonical));
    }

    [DataTestMethod]
    [DataRow("guerriero", "Fighter", "Guerriero")]
    [DataRow("mago", "Magician", "Mago")]
    [DataRow("ladro", "Thief", "Ladro")]
    [DataRow("arciere", "Archer", "Arciere")]
    public void Italian_OccupationsNormalizeAndTranslate(string input, string canonical, string display)
    {
        var messages = LoadItalian();
        string padded = "  " + input.ToUpperInvariant() + "  ";
        Assert.IsTrue(messages.IsValidOccupation(padded));
        Assert.AreEqual(canonical, messages.NormalizeOccupation(padded));
        Assert.AreEqual(display, messages.TranslateOccupationForDisplay(canonical));
    }

    [DataTestMethod]
    [DataRow("long sword", "spada lunga")]
    [DataRow("lightning bolt spell", "incantesimo del fulmine")]
    [DataRow("dagger", "pugnale")]
    [DataRow("long bow", "arco lungo")]
    [DataRow("claws", "artigli")]
    public void Italian_TranslatesWeapons(string canonical, string display)
    {
        Assert.AreEqual(display, LoadItalian().TranslateWeaponForDisplay(canonical));
    }

    [TestMethod]
    public void Italian_PromptsPreserveGameplayCommands()
    {
        var messages = LoadItalian();
        foreach (string key in new[] { "roll_strength", "roll_agility", "roll_health", "roll_invalid" })
            StringAssert.Contains(messages.GetMessage(key), "'roll'");
        foreach (string command in new[] { "'n'", "'s'", "'e'" })
            StringAssert.Contains(messages.GetMessage("path_prompt_full"), command);
        foreach (string key in new[] { "attack_prompt", "dragon_encounter_prompt" })
            foreach (string command in new[] { "'a'", "'r'" })
                StringAssert.Contains(messages.GetMessage(key), command);
        Assert.IsFalse(messages.IsValidRace(null));
        Assert.IsFalse(messages.IsValidOccupation("invalid"));
    }
}

[TestClass]
[DoNotParallelize]
public class ItalianConsoleTests
{
    [DataTestMethod]
    [DataRow("1", "Welcome to Choose Your Adventure Game")]
    [DataRow("2", "Bienvenido al Juego de Elige Tu Aventura")]
    [DataRow("3", "Bienvenue dans Choisissez Votre Aventure")]
    [DataRow("4", "Benvenuto nel gioco Scegli la tua avventura")]
    [DataRow("invalid", "Welcome to Choose Your Adventure Game")]
    public void LanguageSelection_LoadsChosenMenu(string choice, string welcome)
    {
        var originalIn = Console.In;
        var originalOut = Console.Out;
        using var writer = new StringWriter();
        try
        {
            Console.SetIn(new StringReader(choice + "\n2\n"));
            Console.SetOut(writer);
            new Game().StartGame();
            StringAssert.Contains(writer.ToString(), "4. Italiano");
            StringAssert.Contains(writer.ToString(), welcome);
            if (choice == "4")
                StringAssert.Contains(writer.ToString(), "L'avventura finisce. Arrivederci.");
        }
        finally
        {
            Console.SetIn(originalIn);
            Console.SetOut(originalOut);
        }
    }

    [DataTestMethod]
    [DataRow(true, false, "victory")]
    [DataRow(false, false, "defeat")]
    [DataRow(false, true, "retreat")]
    public void Italian_EndGameDisplaysTranslatedOutcome(bool won, bool retreated, string key)
    {
        var messages = new Messages();
        messages.SetCurrentLanguage("Italian");
        messages.ReadDictionary();
        var originalOut = Console.Out;
        using var writer = new StringWriter();
        try
        {
            Console.SetOut(writer);
            new Game().EndGame(won, retreated, messages);
            Assert.AreEqual(messages.GetMessage(key) + Environment.NewLine, writer.ToString());
        }
        finally { Console.SetOut(originalOut); }
    }
}
