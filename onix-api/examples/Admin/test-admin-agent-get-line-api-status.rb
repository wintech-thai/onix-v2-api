#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env("../.env")

orgId = ENV['API_ORG']
keyFile = ".token"

###
agentId = "ed50b202-5db8686d6f-5776j"
apiUrl = "admin-api/AdminAgent/org/global/action/GetLineApiAgentStatus/#{agentId}"

param = nil

token = File.read(keyFile)

ENV['API_KEY'] = nil
ENV['ACCESS_TOKEN'] = token

result = make_request(:get, apiUrl, param)
puts(result.to_json)
