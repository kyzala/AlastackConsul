# Alastack.Consul Samples

## Setup Consul

```bash
consul agent -server -bootstrap -bind 127.0.0.1 -client 0.0.0.0 -config-file ./config -data-dir ./data -ui
```

## Samples

### Sample.Common

Config options definitions for all samples.

### ConsoleSample and WorkerServiceSample

Using Consul Key/Value configuration with Alastack.Consul in the Console application.

Create folders and keys in Consul Key/Value store. Copy config content data from related files in ConsoleSample project.

- **< Key / values** / alastack / sample / public / default / loggings.json
- **< Key / values** / alastack / sample / public / default / config.json

### AspNetSample and DynamicInstanceSample

Using Consul Key/Value configuration and Service Discovery with Alastack.Consul in AspNetCore.

Create folders and keys in Consul Key/Value store. Copy config content data from related files in AspNetSample project.

- **< Key / values** / aspnetsample / public1 / default / loggings.json
- **< Key / values** / aspnetsample / public1 / group1 / config1.json
- **< Key / values** / aspnetsample / public1 / group1 / config2.json
