# Connection Request

This section explains the concept of "Reliable UDP".


Previous Section: [Peer Object](/PeerObject.md)

## Message reliability and channels

Fenrir Multiplayer supports both reliable and unreliable communication.

When sending Requests or Events, optional **Channel** and **Message Delivery** parameters can be set:

```csharp

// Send realiable message using the 1st channel. Packets won't be dropped, won't be duplicated, will arrive in order.
networkClient.Peer.SendRequest(new PingRequest(), channel: 1, deliveryMethod: MessageDeliveryMethod.ReliableOrdered);

// Send unreliable message using 2nd channel. Packets can be dropped, can be duplicated, can arrive in no specific order.
networkClient.Peer.SendRequest(new PingRequest(), channel: 2, deliveryMethod: MessageDeliveryMethod.Unreliable);
```


Next Section: [Serialization](/Serialization.md)

