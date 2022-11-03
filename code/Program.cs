

using System;
using System.Collections.Generic;
using Tensorflow;
using Tensorflow.Keras;
using static Tensorflow.Binding;
using static Tensorflow.KerasApi;
using Tensorflow.Keras.Utils;
using System.IO;
using System.Diagnostics;
using Tensorflow.Keras.Engine;

namespace TFdotNET.ImagineClassication
{
    class Program
    {
        static int batch_size = 20;
        static int epochs = 10;
        //static Shape img_dim = (180, 180);
        static IDatasetV2 train_ds, val_ds;
        static Model model;
        static void LoadImage(string dataPath)
        {
            string data_dir =  dataPath ;  //"./imgs/flower";

            train_ds = keras.preprocessing.image_dataset_from_directory(data_dir,
                validation_split: 0.2f,
                subset: "training",
                seed: 123,
                image_size: (180, 180),
                batch_size: batch_size);

            val_ds = keras.preprocessing.image_dataset_from_directory(data_dir,
            validation_split: 0.2f,
            subset: "validation",
            seed: 123,
            image_size: (180, 180),
            batch_size: batch_size);

            train_ds = train_ds.shuffle(1000).prefetch(buffer_size: -1);
            val_ds = val_ds.prefetch(buffer_size: -1);


        }
        static void BuildModel()
        {
            int num_classes = 5;
            // var normalization_layer = tf.keras.layers.Rescaling(1.0f / 255);
            var layers = keras.layers;
            model = keras.Sequential(new List<ILayer>
            {
                layers.Rescaling(1.0f / 255, input_shape: (180, 180, 3)),
                layers.Conv2D(16, 3, padding: "same", activation: keras.activations.Relu),
                // layers.MaxPooling2D(),
                // layers.Conv2D(32, 3, padding: "same", activation: "relu"),
                // layers.MaxPooling2D(),
                // layers.Conv2D(64, 3, padding: "same", activation: "relu"),
                layers.MaxPooling2D(),
                layers.Flatten(),
                layers.Dense(128, activation: keras.activations.Relu),
                layers.Dense(num_classes)
            });

            model.compile(optimizer: keras.optimizers.Adam(),
                loss: keras.losses.SparseCategoricalCrossentropy(from_logits: true),
                metrics: new[] { "accuracy" });

            model.summary();
        }

        static void TrainModel()
        {
            model.fit(train_ds, validation_data: val_ds, epochs: epochs);
        }

        static void saveModel(string outputPath)
        {   

            
            string str_directory = Environment.CurrentDirectory.ToString();
            string demo = Directory.GetParent(str_directory).FullName;  

            Console.WriteLine("123:"+demo);
            // try
            // {
            //     if (!Directory.Exists(outputPath))
            //     {
            //         // Try to create the directory.
            //         DirectoryInfo di = Directory.CreateDirectory(outputPath);
            //         model.save_weights(outputPath+"/models.h5");
            //     // }
            // }
            // catch (IOException ioex)
            // {
            //     Console.WriteLine(ioex.Message);
            // }
            // bool folderExists = Directory.Exists(Server.MapPath(outputPath));
            // if (!folderExists)
            //     Directory.CreateDirectory(Server.MapPath(path));
            model.save_weights(demo + "/outputs/models.h5");
        }

        static void Main(string[] args)
        {
            var argumentsPath =  "./arguments.json";
            var argumentParser = new ArgumentParser(new FileInfo(argumentsPath), args);


            var dataPath = argumentParser.TryGetValueString("dataPath", out string dP) ? dP : null;
            var outputPath = argumentParser.TryGetValueString("outputPath", out string oP) ? oP : null;


            tf.enable_eager_execution();
            
            var sw = new Stopwatch();
            try
            {
                    sw.Restart();
                    LoadImage(dataPath);
                    BuildModel();
                    TrainModel();
                    saveModel(outputPath);
                    sw.Stop();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            
            keras.backend.clear_session();

        }
    }
}