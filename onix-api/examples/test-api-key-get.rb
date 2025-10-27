#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env(".env")

orgId = ENV['API_ORG']
id = '6978a49b-66a0-4511-b4de-37059caf1c33'

### GetApiKeyById
apiUrl = "api/ApiKey/org/#{orgId}/action/GetApiKeyById/#{id}"
result = make_request(:get, apiUrl, nil)
puts(result)
