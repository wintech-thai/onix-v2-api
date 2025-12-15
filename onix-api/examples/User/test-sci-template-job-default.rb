#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env("../.env")

orgId = ENV['API_ORG']
id = 'e4018225-a6d2-4c78-92c8-4f4f832978a1';

### GetScanItemAction
apiUrl = "api/ScanItemTemplate/org/#{orgId}/action/GetJobDefaultByTemplateId/#{id}"
param = nil

result = make_request(:get, apiUrl, param)
puts(result)

