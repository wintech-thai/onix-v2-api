#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env("../.env")

keyFile = ".token"
rowId = 'ใส่-payment-request-id-หรือ-transaction-id-ที่นี่'

###
apiUrl = "admin-api/AdminAuditTrail/org/global/action/GetAuditTrailByRowId/#{rowId}"
param = nil

token = File.read(keyFile)

ENV['API_KEY'] = nil
ENV['ACCESS_TOKEN'] = token

result = make_request(:get, apiUrl, param)
puts(result.to_json)
