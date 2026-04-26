using TorchSharp.Modules;
using static TorchSharp.torch;
using static TorchSharp.torch.nn;

namespace AzulAIAndGameState.Players.MCTS_NN.Networks
{
    public class PolicyNetwork : Module
    { 
        public Adam optimizer;

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

        public PolicyNetwork(string modelPath = "") : base("PolicyOnlyNetwork") {
            fc1 = new Layer(301, 256);
            fc2 = new Layer(256, 256);
            fc3 = new Layer(256, 256);
            fc4 = new Layer(256, 256);
            fc5 = new Layer(256, 256);
            fc6 = new Layer(256, 256);
            fc7 = new Layer(256, 256);
            fc8 = new Layer(256, 256);
            fc9 = new Layer(256, 256);
            fc10 = new Layer(256, 180);

            RegisterComponents();

            if (modelPath != "") {
                load(modelPath);
            }
            optimizer = optim.Adam(parameters(), lr: 0.001);
        }

        public Tensor Call(Tensor x) {
            var residual = fc1.Forward(x);
            
            x = fc2.Forward(residual);
            x = fc3.Forward(x, residual);
            x = fc4.Forward(x, residual);
            x = fc5.Forward(x, residual);
            x = fc6.Forward(x, residual);

            x = fc6.ForwardWithRelu(x, residual);
            x = fc7.ForwardWithRelu(x, residual);
            x = fc8.ForwardWithRelu(x, residual);
            x = fc9.ForwardWithRelu(x, residual);
            
            x = fc10.Forward(x);

            return x;
        }

        public void TrainWithLoss(Tensor loss) {
            optimizer.zero_grad();
            float lossValue = loss.mean().item<float>();
            //Console.WriteLine("Backpropagating: " + lossValue);
            loss.mean().backward();
            optimizer.step();
        }
    }

    internal class Layer {
        private Linear fc;

        public Layer(int input, int output) {
            fc = Linear(input, output);
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

        public Tensor ForwardWithRelu(Tensor x, Tensor residual) {
            x = fc.forward(x);
            x = x + residual;
            x = functional.relu(x);
            return x;
        }
    }
}