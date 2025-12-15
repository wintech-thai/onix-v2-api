#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env("../.env")

orgId = ENV['API_ORG']
id = 'd645db7d-6738-412e-8853-4bd1e1d526ad';

### GetScanItemAction
apiUrl = "api/ScanItemTemplate/org/#{orgId}/action/GetScanItemTemplateById/#{id}"
param = nil

result = make_request(:get, apiUrl, param)
puts(result)

