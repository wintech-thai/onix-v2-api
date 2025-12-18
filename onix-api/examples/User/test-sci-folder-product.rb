#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env("../.env")

orgId = ENV['API_ORG']
id = 'a712af4e-bb2f-49eb-8632-1516ce180a8d'
productId = 'a4553993-cf07-4a04-81d5-e7743bab5e55'

apiUrl = "api/ScanItemFolder/org/#{orgId}/action/AttachScanItemFolderToProduct/#{id}/#{productId}"
param = nil

result = make_request(:post, apiUrl, param)
puts(result)
