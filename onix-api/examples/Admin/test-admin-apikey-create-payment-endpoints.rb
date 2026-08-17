#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env("../.env")

keyFile = ".token"
userOrgId = "ppm-alfa888"
roles = URI.encode_www_form_component("PAYIN_REQUEST,PAYIN_REQUEST_P2P,PAYOUT_REQUEST")

###
apiUrl = "admin-api/AdminApiKey/org/global/action/CreatePaymentEndpointsApiKey/#{userOrgId}/#{roles}"
param = {}

token = File.read(keyFile)

ENV['API_KEY'] = nil
ENV['ACCESS_TOKEN'] = token

result = make_request(:post, apiUrl, param)
puts(result.to_json)
