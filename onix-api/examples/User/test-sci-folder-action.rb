#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env("../.env")

orgId = ENV['API_ORG']
id = '131203c0-e72e-4f02-81a0-4b85d1fb268e'
actionId = '5f52ce98-72f2-4e93-b3aa-96d8b7c4eca2'

apiUrl = "api/ScanItemFolder/org/#{orgId}/action/AttachScanItemFolderToAction/#{id}/#{actionId}"
param = nil

result = make_request(:post, apiUrl, param)
puts(result)
