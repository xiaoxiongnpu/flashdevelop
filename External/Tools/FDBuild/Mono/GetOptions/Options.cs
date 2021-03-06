namespace Mono.GetOptions
{
    public class Options
    {
        // Methods
        public Options()
        {
            ParsingMode = OptionsParsingMode.Both;
            BreakSingleDashManyLettersIntoManyOptions = false;
            EndOptionProcessingWithDoubleDash = true;
        }

        [Option("Display version and licensing information", 'V', "version")]
        public virtual WhatToDoNext DoAbout() => optionParser.DoAbout();

        [Option("Show this help list", '?', "help")]
        public virtual WhatToDoNext DoHelp() => optionParser.DoHelp();

        [Option("Show an additional help list", "help2")]
        public virtual WhatToDoNext DoHelp2() => optionParser.DoHelp2();

        [Option("Show usage syntax and exit", "usage")]
        public virtual WhatToDoNext DoUsage() => optionParser.DoUsage();

        public void ProcessArgs(string[] args)
        {
            optionParser = new OptionList(this);
            RemainingArguments = optionParser.ProcessArgs(args);
        }

        public void ShowBanner() => optionParser.ShowBanner();

        // Properties
        [Option("Show verbose parsing of options", "verbosegetoptions", SecondLevelHelp=true)]
        public bool VerboseParsingOfOptions
        {
            set => OptionDetails.Verbose = value;
        }
        
        // Fields
        public bool BreakSingleDashManyLettersIntoManyOptions;
        public bool EndOptionProcessingWithDoubleDash;
        OptionList optionParser;
        public OptionsParsingMode ParsingMode;
        public string[] RemainingArguments;
    }
}