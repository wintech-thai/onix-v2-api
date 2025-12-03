#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env("../.env")

orgId = ENV['API_ORG']
id = '7daf5a08-7df8-4434-8204-457bc9782a25'

apiUrl = "api/Voucher/org/#{orgId}/action/DeleteVoucherById/#{id}"
result = make_request(:delete, apiUrl, nil)
puts(result)
