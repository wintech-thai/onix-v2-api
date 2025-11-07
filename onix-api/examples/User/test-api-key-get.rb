#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env("../.env")

orgId = ENV['API_ORG']
id = 'c916e634-19a9-48e9-97c1-3d1a1455eb6e'

### GetApiKeyById
apiUrl = "api/ApiKey/org/#{orgId}/action/GetApiKeyById/#{id}"
result = make_request(:get, apiUrl, nil)
puts(result)
