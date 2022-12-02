# TibberSubscription

<p>A simple console application to subscribe to live data from Tibber API using <a href=https://github.com/tibber/Tibber.SDK.NET>Tibber.SDK.NET</a> to connect and receive live data feed and <a href=https://github.com/dotnet/MQTTnet>MQTTnet</a> to broadcast to MQTT broker.</p>

![image](https://user-images.githubusercontent.com/73751609/204908359-fba1ee93-041c-4968-8172-0233b1268210.png)

<h1>Resources.xml</h1>
<p>This file must be configured and placed in same folder as the program</p>
<p>Enter required data following the comments in the file.</p>
<h4>Tibber</h4>
<p>You can get Tibber API key and HomeId from <a href=https://developer.tibber.com/>Tibber Developer page</a></p>
<h4>MQTT</h4>
<p>An MQTT broker address and port is required, as IP or hostname</p>
<p>The delay entry is to reduce amount of data sent to MQTT</p>
 <ul>
  <li>A value of 1 is same update speed as Tibber stream</li>
  <li>A higher value will reduce the update interval sent to MQTT</li>
</ul>
<h4>Topics</h4>
<p>Topics defined in this file are the ones that are enabled in the <a href=https://github.com/tibber/Tibber.SDK.NET>Tibber.SDK.NET</a></p>
<p>To enable a subscription topic to be broadcasted to MQTT, change the "enabled" attribute to "true"</p>
