#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env("../.env")

keyFile = ".token"
merchantId = "cfd098d5-ed0b-4acd-a10e-9fbb9e3d42c0"

###
apiUrl = "admin-api/AdminMerchant/org/global/action/GetMerchantPaymentEndpoints/#{merchantId}"
param = nil

token = File.read(keyFile)

ENV['API_KEY'] = nil
ENV['ACCESS_TOKEN'] = token

result = make_request(:get, apiUrl, param)
puts(result.to_json)
