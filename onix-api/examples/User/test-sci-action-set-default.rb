#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env("../.env")

orgId = ENV['API_ORG']
id = '46272c36-8772-476e-bb67-b2d52e55c620';

### GetScanItemAction
apiUrl = "api/ScanItemAction/org/#{orgId}/action/SetDefaultScanItemActionById/#{id}"
param = nil

result = make_request(:post, apiUrl, param)
puts(result)

