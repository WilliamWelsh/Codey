namespace Codey
{
    public class CommandResult
    {
        public string Text { get; set; }
        public bool wasSuccessful { get; set; }

        // Empty constructor for unsuccessful results
        public CommandResult() { }

        // A string with a bool for unsuccessful results with a message
        public CommandResult(string result, bool wasSuccessful)
        {
            Text = result;
            this.wasSuccessful = wasSuccessful;
        }

        // Just a string for successful results
        public CommandResult(string result)
        {
            Text = result;
            wasSuccessful = true;
        }
    }
}