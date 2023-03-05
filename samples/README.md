# Alastack.Consul Samples

## Setup Consul

```bash
consul agent -server -bootstrap -bind 127.0.0.1 -client 0.0.0.0 -config-file ./config -data-dir ./data -ui
```

## Samples

### Sample.Common

Config options definitions for all samples.

### ConsoleSample

Using Consul Key/Value configuration with Alastack.Consul in the Console application.

Create folders and keys in Consul Key/Value store. Copy config content data from related files in ConsoleSample project.

- **< Key / values** / alastack / sample / ns.default / gp.default / loggings.json
- **< Key / values** / alastack / sample / ns.default / gp.default / config.json

### WorkerServiceSample

Using Consul Key/Value configuration with Alastack.Consul in the HostedService.

Create folders and keys in Consul Key/Value store. Copy config content data from related files in WorkerServiceSample project.

- **< Key / values** / alastack / sample / ns.default / gp.default / loggings.json
- **< Key / values** / alastack / sample / ns.default / gp.default / config.json

### AspNetSample

Using Consul Key/Value configuration and Service Discovery with Alastack.Consul in AspNetCore.

Create folders and keys in Consul Key/Value store. Copy config content data from related files in AspNetSample project.

- **< Key / values** / alastack / aspnetsample / ns.default / gp.default / loggings.json
- **< Key / values** / alastack / aspnetsample / ns.default / gp.group1 / config1.json
- **< Key / values** / alastack / aspnetsample / ns.default / gp.group1 / config2.json