# iot-edge-mlnet-text-classification-test

Demonstration of integrating the ML.Net Text Classification into Azure IoT Edge.

This example is part of a blog at https://blog.vandeveldne-online.com

## Modules

This project contains two modules:

1. iot-edge-mlnet-text-classification
2. iot-edge-mlnet-text-classification-test

The first module takes a message on input 'input1' and write the outcome to output 'output1'. The outcome is also shown in the console log.

The second module is there to test the text classification. It supports a method for sending multiple test comments.

#### Module iot-edge-mlnet-text-classification

Input message format:

```
public class Request
{
    public string comment {get; set;}
}
```

Output message format:

```
public class Response
{
    public string comment {get; set;}
    public string prediction {get; set;}

    public Score[] scores {get; set;}
}

public class Score
{
    public float entry {get; set;}
}
```

### Module iot-edge-mlnet-text-classification-test

Direct method 'meassureSentiment' body format:

```
public class Request
{
    public string comment {get; set;}
}
```

Output message format:

```
public class Response
{
    public string comment {get; set;}
}
```

## Route

The two modules are conencted with a route:

    FROM /messages/modules/test/outputs/output1 INTO BrokeredEndpoint("/modules/text/inputs/input1")

This results in:

![](https://github.com/sandervandevelde/iot-edge-mlnet-text-classification-test/blob/main/assets/flow.png)

## Contribute

Feel free to contribite to this project.


