#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env("../.env")

orgId = ENV['API_ORG']

param = nil
id = 'd8997e61-7482-4972-9ee7-d08b6551287b'
folderId = 'bd8033ad-d2e6-4537-9294-70b5934a999a'

apiUrl = "api/ScanItem/org/#{orgId}/action/MoveScanItemToFolder/#{id}/#{folderId}"
result = make_request(:post, apiUrl, param)
puts(result)
