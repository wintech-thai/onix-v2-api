#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env("../.env")

# SCB bank account ID
bankAccountId = '2dc03b05-d67a-4976-87a6-ccebcc0c3c55'

# ต้องตรงกับ payment request ที่สร้าง QR ไว้
refId1 = '20260627165838'

apiUrl = "admin-api/AdminPaymentTx/org/global/action/SubmitScbPaymentConfirmation/#{bankAccountId}"

# field จำลองตามที่ SCB ส่งมาจริง
param = {
  transactionId: "SIMULATE-#{Time.now.strftime('%Y%m%d%H%M%S')}",
  amount: "4.72",
  transactionDateandTime: Time.now.strftime('%Y-%m-%dT%H:%M:%S.000+07:00'),
  currencyCode: "THB",
  transactionType: "Bill Payment",
  billPaymentRef1: refId1,
  billPaymentRef2: refId1,
  billPaymentRef3: "CQH#{refId1}",
  payerName: "Khanatorn Sosang",
  payerProxyId: "4917440001",
  channelCode: "PMH",
  paymentMethod: "QR30",
}

# [AllowAnonymous] ไม่ต้องส่ง auth
ENV['API_KEY'] = nil
ENV['ACCESS_TOKEN'] = nil

result = make_request(:post, apiUrl, param)
puts(result)
