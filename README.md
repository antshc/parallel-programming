# A comprehensive course teaching you how to write asynchronous C# code with the Task Parallel Library and PLINQ.
[Course](https://ciklum.udemy.com/course/write-asynchronous-csharp-code-with-task-parallel-library-and-plinq/learn/lecture/3699912#overview)
* Run long running task not in thread pool, use LongRunning enum parameter for that.
* You can use state task argument, it values displays in the debbuger
* Use task to handle up to 10k unit of works, where you do not need to do map reduce operations. 
* Parralle library has optimization that helps to handle millions of unit of works using thousands of tasks. 
* Plinq does the same as parrallel library but also has automatic map, reduce methods .
* Plinq doesn't preserve the order of the items.
* Use AsOrdered  method to have items in preserve order.



# Dataflow areas of using
## Sensor data processing
Let's consider a scenario where you have a stream of sensor data coming from multiple devices, and you need to process and analyze this data in real-time. 
Dataflow can be a great fit for building such a data processing pipeline. 
## Web crawler
A simple web crawler that fetches and processes web pages concurrently.
## Image Processing Pipeline
Suppose you have a requirement to process a batch of images asynchronously, applying multiple filters and transformations to each image. 
You can use Dataflow to create a pipeline for this task efficiently.
## Real-time Data Processing Pipeline
Let's say you have a real-time data processing requirement where you receive a continuous stream of data from various sources, and you need to process this data concurrently and efficiently. 
Dataflow can be a good fit for building such pipelines.

