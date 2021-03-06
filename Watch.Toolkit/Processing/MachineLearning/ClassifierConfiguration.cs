﻿using System;
using System.Collections.Generic;
using System.IO;

namespace Watch.Toolkit.Processing.MachineLearning
{
    public class ClassifierConfiguration
    {
        public List<String> Labels { get; set; }
        public string TrainingDataPath { get; set; }

        public ClassifierConfiguration(List<string> labels, string trainingDataPath)
        {
            Labels = labels;
            TrainingDataPath = trainingDataPath;

            if (!File.Exists(trainingDataPath))
            {
                throw new IOException("File with training data was not found.");
            }
        }

        public int GetLabelValue(string label)
        {
            return Labels.FindIndex(a => a == label);
        }

        public string GetLabel(int value)
        {
            return Labels[value];
        }
    }
}
