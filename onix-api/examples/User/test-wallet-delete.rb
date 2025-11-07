#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env("../.env")

orgId = ENV['API_ORG']
id = '4d9fdd49-307b-428c-8c2e-927d27ef258d'

param =  nil

apiUrl = "api/Point/org/#{orgId}/action/DeleteWalletById/#{id}"
result = make_request(:delete, apiUrl, param)

puts(result)
