using TorchSharp;
using TorchSharp.Modules;
using static TorchSharp.torch;
using static TorchSharp.torch.nn;

namespace AzulAIAndGameState.Players.MCTS_NN.Networks
{
    public class PolicyNetwork : Module, INetwork
    { 
        public Adam optimizer;
        public Device Device { get; private set; }

        private Layer fc1;
        private Layer fc2;
        private Layer fc3;
        private Layer fc4;
        private Layer fc5;
        private Layer fc6;
        private Layer fc7;
        private Layer fc8;
        private Layer fc9;
        private Layer fc10;
        private Layer fc11;
        private Layer fc12;
        private Layer fc13;
        private Layer fc14;

        public PolicyNetwork(string modelPath = "") : base("PolicyOnlyNetwork") {
            if (cuda.is_available())
                Device = CUDA;
            else
                Device = CPU;
            
            fc1 = new Layer(301, 512);
            fc2 = new Layer(512, 512);
            fc3 = new Layer(512, 512);
            fc4 = new Layer(512, 512);
            fc5 = new Layer(512, 512);
            fc6 = new Layer(512, 512);
            fc7 = new Layer(512, 512);
            fc8 = new Layer(512, 512);
            fc9 = new Layer(512, 512);
            fc11 = new Layer(512, 512);
            fc12 = new Layer(512, 512);
            fc13 = new Layer(512, 512);
            fc14 = new Layer(512, 512);
            fc10 = new Layer(512, 180);

            RegisterComponents();

            if (modelPath != "") {
                load(modelPath).to(Device);
            }
            optimizer = optim.Adam(parameters(), lr: 0.001);
        }

        public Tensor Call(Tensor x) {
            x = fc1.Forward(x);
            
            var x1 = fc2.ForwardWithRelu(x);
            var x2 = fc3.ForwardWithRelu(x1);
            var x3 = fc4.ForwardWithRelu(x2);
            var x4 = fc5.ForwardWithRelu(x3);

            x = fc6.ForwardWithRelu(x, x4);
            x = fc7.ForwardWithRelu(x, x3);
            x = fc8.ForwardWithRelu(x, x2);
            x = fc9.ForwardWithRelu(x, x1);

            x = fc11.ForwardWithRelu(x, x4);
            x = fc12.ForwardWithRelu(x, x3);
            x = fc13.ForwardWithRelu(x, x2);
            x = fc14.ForwardWithRelu(x, x1);

            x = fc10.Forward(x);

            return x;
        }

        public void TrainWithLoss(Tensor loss) {
            optimizer.zero_grad();
            loss.backward();
            optimizer.step();
        }
        public void Save(string path) => save(path);
        public void Load(string path) => load(path);
    }

    internal class Layer : Module {
        private Linear fc;

        public Layer(int input, int output) : base("LinearWrapper") {
            fc = Linear(input, output);

            RegisterComponents();
        }

        public Tensor Forward(Tensor x) {
            x = fc.forward(x);
            return x;
        }
        public Tensor Forward(Tensor x, Tensor residual) {
            x = fc.forward(x);
            x = x + residual;
            return x;
        }
        public Tensor ForwardWithRelu(Tensor x)
        {
            x = fc.forward(x);
            x = functional.relu(x);
            return x;
        }
        public Tensor ForwardWithRelu(Tensor x, Tensor residual) {
            x = fc.forward(x);
            x = x + residual;
            x = functional.relu(x);
            return x;
        }
    }
}