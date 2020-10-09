# iot-edge-mlnet-text-classification-test

Demonstration of integrating the ML.Net Text Classification into Azure IoT Edge

## Modules

This project contains two modules:

1. svelde/iot-edge-mlnet-text-classification
2. svelde/iot-edge-mlnet-text-classification-test

The first module takes a message on input 'input1' and write the outcome to output 'output1'. The outcome is also shown in the console log.

The second module is there to test the text classification. It supports a method for sending multiple test comments.

## Route

The two modules are conencted with a route:

This results in:

![](https://github.com/sandervandevelde/iot-edge-mlnet-text-classification-test/blob/main/assets/flow.png)


