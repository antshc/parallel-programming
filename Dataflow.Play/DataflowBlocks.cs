using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Dataflow.Play
{
    public class DataflowBlocks
    {
        [Fact]
        public void TransformManyBlockTest()
        {
            // Input many single object, split single object to smaller blocks,
            // concat blocks from each Post "object" to a single pipe of blocks,
            // read each block on Receive
            var transformManyBlock = new TransformManyBlock<string, char>(
   s =>
   {
       return s.ToCharArray();
   });

            // Post two messages to the first block.
            transformManyBlock.Post("Hello");
            transformManyBlock.Post("World");

            // Receive all output values from the block.
            for (int i = 0; i < ("Hello" + "World").Length; i++)
            {
                Console.WriteLine(transformManyBlock.Receive());
            }

        }

        [Fact]
        public void TransformBlockTest()
        {

            // Input many single blocks,
            // concat blocks from each Post "block" to a single pipe of blocks,
            // read each block on Receive
            var transformBlock = new TransformBlock<int, double>(n => Math.Sqrt(n));

            // Post several messages to the block.
            transformBlock.Post(10);
            transformBlock.Post(20);
            transformBlock.Post(30);

            // Read the output messages from the block.
            for (int i = 0; i < 3; i++)
            {
                Console.WriteLine(transformBlock.Receive());
            }
        }

        [Fact]
        public void ActionBlockTest()
        {

            var actionBlock = new ActionBlock<int>(n =>
            {
                Console.WriteLine(n);
            });

            // Post several messages to the block.
            for (int i = 0; i < 3; i++)
            {
                actionBlock.Post(i * 10);
            }

            // Set the block to the completed state and wait for all
            // tasks to finish.
            actionBlock.Complete();
            actionBlock.Completion.Wait();
        }

        [Fact]
        public void BufferBlockTest()
        {
            // Create a BufferBlock<int> object.
            var bufferBlock = new BufferBlock<int>();

            // Post several messages to the block.
            for (int i = 0; i < 3; i++)
            {
                bufferBlock.Post(i);
            }

            // Receive the messages back from the block.
            for (int i = 0; i < 3; i++)
            {
                Console.WriteLine(bufferBlock.Receive());
            }

            /* Output:
               0
               1
               2
             */
        }
    }
}
