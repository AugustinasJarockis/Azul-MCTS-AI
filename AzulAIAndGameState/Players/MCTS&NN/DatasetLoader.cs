using TorchSharp;
using static TorchSharp.torch;

namespace AzulAIAndGameState.Players.MCTS_CNN
{
    internal class DatasetLoader
    {
        static Random random = new();

        private float[][] data;

        private Tensor[] states;
        private long[] policy;
        private float[] values;

        public int DatasetSize => states.Length;

        int batchStart = 0; 
        public DatasetLoader(string filename) {
            string text = File.ReadAllText(filename).ToString();

            string[] splitData = text.Split('\n', StringSplitOptions.RemoveEmptyEntries).ToArray();
            data = splitData.Select(s => s.Split(';', StringSplitOptions.RemoveEmptyEntries).Select(c => float.Parse(c)).ToArray()).ToArray();

            random.Shuffle(data);
            SeparateStatesPoliciesAndValues();
        }

        private DatasetLoader(float[][] data, Tensor[] states, long[] policy, float[] values) {
            this.data = data;
            this.states = states;
            this.policy = policy;
            this.values = values;
        }

        public void Shuffle() {
            random.Shuffle(data);
            batchStart = 0;
            SeparateStatesPoliciesAndValues();
        }

        public DatasetLoader SplitTestPart(int testPartSize) {
            if (testPartSize > states.Length)
                throw new Exception("Test part cannot be larger than entire set");
            
            if (batchStart > states.Length)
                batchStart = 0;

            float[][] testData = data.Take(testPartSize).ToArray();
            data = data.Skip(testPartSize).ToArray();
            Tensor[] testStates = states.Take(testPartSize).ToArray();
            states = states.Skip(testPartSize).ToArray();
            long[] testPolicy = policy.Take(testPartSize).ToArray();
            policy = policy.Skip(testPartSize).ToArray();
            float[] testValue = values.Take(testPartSize).ToArray();
            values = values.Skip(testPartSize).ToArray();

            return new (testData, testStates, testPolicy, testValue);
        }

        public (Tensor[], long[], float[]) GetBatch(int batchSize) {
            int toSkip = batchStart;
            batchStart += batchSize;
            if (batchStart > states.Length - batchSize)
                batchStart = 0;

            return (
                states.Skip(batchStart).Take(batchSize).ToArray(), 
                policy.Skip(batchStart).Take(batchSize).ToArray(), 
                values.Skip(batchStart).Take(batchSize).ToArray()
                );
        }

        private void SeparateStatesPoliciesAndValues() {
            states = data.Select(r => r.Take(r.Length - 2).ToArray().ToTensor([r.Length -2 ])).ToArray();
            policy = data.Select(r => (long)r[^2]).ToArray();
            values = data.Select(r => r[^1] > 0 ? 1.0f : (r[^1] < 0 ? -1.0f : 0.0f)).ToArray();
        }
    }
}
