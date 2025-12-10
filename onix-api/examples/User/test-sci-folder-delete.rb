#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env("../.env")

orgId = ENV['API_ORG']
id = '3cb1d2df-8ca7-423b-868a-a8412b9ce639'

apiUrl = "api/ScanItemFolder/org/#{orgId}/action/DeleteScanItemFolderById/#{id}"
param = nil

result = make_request(:delete, apiUrl, param)
puts(result)
